using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM;
using Kingmaker.Blueprints;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using ObservableCollections;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVM : ViewModel, ILevelUpManagerUIHandler, ISubscriber, ICharGenCloseHandler, INetLobbyPlayersHandler
{
	public readonly CharGenContext CharGenContext;

	private readonly List<(SelectionState, CharGenPhaseBaseVM)> m_PhasesList = new List<(SelectionState, CharGenPhaseBaseVM)>();

	private readonly ObservableList<CharGenPhaseBaseVM> m_PhasesCollection = new ObservableList<CharGenPhaseBaseVM>();

	private readonly ReactiveProperty<CharGenPhaseBaseVM> m_CurrentPhaseVM = new ReactiveProperty<CharGenPhaseBaseVM>();

	private readonly ReactiveProperty<PortraitVM> m_PortraitVM = new ReactiveProperty<PortraitVM>();

	private readonly ReactiveProperty<CharacterVisualSettingsVM> m_VisualSettingsVM = new ReactiveProperty<CharacterVisualSettingsVM>();

	private readonly ReactiveProperty<bool> m_CurrentPhaseIsCompleted = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_NextButtonHint = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_ShouldShowVisualSettings = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsInCharscreen = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<bool> m_CheckCoopControls = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<bool> m_IsMainCharacter = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>();

	private readonly Action m_CloseAction;

	private readonly Action<BaseUnitEntity> m_CompleteAction;

	private readonly CompositeDisposable m_PhaseSubscriptions = new CompositeDisposable();

	private bool m_PortraitSynchronizingInProgress;

	private bool m_IsPlaningMode;

	private LevelUpManager m_PlaningLevelUpManager;

	private CharGenPhaseBaseVM m_LastPhase;

	private IDisposable m_PortraitSubscription;

	private IDisposable m_PhaseIsCompletedSubscription;

	private bool m_IsPhasesDirty;

	private readonly bool m_IsCustomCompanionChargen;

	public readonly SelectionGroupRadioVM<CharGenPhaseBaseVM> PhasesSelectionGroupRadioVM;

	private CharGenMode m_Mode => CharGenContext.CharGenConfig.Mode;

	private bool m_IsLevelUpMode => m_Mode == CharGenMode.LevelUp;

	public InfoSectionVM ChargenInfoSectionVM { get; }

	public InfoSectionVM LevelUpInfoSectionVM { get; }

	public PartyStatsOverviewVM PartyStatsOverviewVM { get; }

	public ChargenProgressionVM ProgressionVM { get; }

	public CharInfoExperienceVM Experience { get; }

	public ObservableList<CharGenPhaseBaseVM> PhasesCollection => m_PhasesCollection;

	public ReadOnlyReactiveProperty<CharGenPhaseBaseVM> CurrentPhaseVM => m_CurrentPhaseVM;

	public ReadOnlyReactiveProperty<PortraitVM> PortraitVM => m_PortraitVM;

	public ReadOnlyReactiveProperty<CharacterVisualSettingsVM> VisualSettingsVM => m_VisualSettingsVM;

	public ReadOnlyReactiveProperty<bool> CurrentPhaseIsCompleted => m_CurrentPhaseIsCompleted;

	public ReadOnlyReactiveProperty<string> NextButtonHint => m_NextButtonHint;

	public ReadOnlyReactiveProperty<bool> ShouldShowVisualSettings => m_ShouldShowVisualSettings;

	public ReadOnlyReactiveProperty<bool> IsInCharscreen => m_IsInCharscreen;

	public bool CurrentPhaseCanInterrupt => CurrentPhaseVM.CurrentValue?.CanInterruptChargen ?? false;

	public CharGenPhaseBaseVM LastPhase => m_LastPhase;

	public Observable<bool> CheckCoopControls => m_CheckCoopControls;

	public ReadOnlyReactiveProperty<bool> IsMainCharacter => m_IsMainCharacter;

	public CharGenVM(CharGenConfig config, Action closeAction, Action<BaseUnitEntity> completeAction, bool isCustomCompanionChargen)
	{
		PFLog.UI.Log($"[{Time.frameCount}] | {Time.realtimeSinceStartup} Start creating CharGenVM");
		m_CloseAction = closeAction;
		m_CompleteAction = completeAction;
		m_IsCustomCompanionChargen = isCustomCompanionChargen;
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		LevelUpInfoSectionVM = new InfoSectionVM().AddTo(this);
		ChargenInfoSectionVM = new InfoSectionVM().AddTo(this);
		PartyStatsOverviewVM = new PartyStatsOverviewVM().AddTo(this);
		CharGenContext = new CharGenContext(config).AddTo(this);
		CharGenContext.SetPregenUnit(null);
		CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager).AddTo(this);
		CharGenContext.IsCustomCharacter.Subscribe(delegate
		{
			m_IsPhasesDirty = true;
			HideVisualSettings();
		}).AddTo(this);
		if (config.Mode == CharGenMode.LevelUp)
		{
			ProgressionVM = new ChargenProgressionVM(CharGenContext.LevelUpManager, m_CurrentPhaseVM).AddTo(this);
			Experience = new CharInfoExperienceVM(m_Unit).AddTo(this);
		}
		UpdatePhases();
		PhasesSelectionGroupRadioVM = new SelectionGroupRadioVM<CharGenPhaseBaseVM>(m_PhasesCollection, m_CurrentPhaseVM).AddTo(this);
		PhasesSelectionGroupRadioVM.TrySelectFirstValidEntity();
		CurrentPhaseVM.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(CharGenPhaseBaseVM phase)
		{
			ClearPhaseSubscription();
			HideVisualSettings();
			if (phase != null)
			{
				m_NextButtonHint.Value = phase.PhaseNextHint;
				phase.IsCompletedAndAvailable.Subscribe(delegate(bool value)
				{
					m_CurrentPhaseIsCompleted.Value = value;
				}).AddTo(m_PhaseSubscriptions);
				phase.ShowVisualSettings.Subscribe(delegate(bool value)
				{
					m_ShouldShowVisualSettings.Value = value;
				}).AddTo(m_PhaseSubscriptions);
				UpdateChargenMusicState(phase);
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			UpdateState();
		}).AddTo(this);
		GameUIState.Instance.CurrentFullScreenUIType.DebounceFrame(1).Subscribe(delegate(FullScreenUIType type)
		{
			m_IsInCharscreen.Value = type == FullScreenUIType.CharacterScreen || type == FullScreenUIType.Inventory;
		}).AddTo(this);
		PFLog.UI.Log($"[{Time.frameCount}] | {Time.realtimeSinceStartup} Finished creating CharGenVM");
	}

	public void SetCurrentPhase(CharGenPhaseBaseVM phaseVM)
	{
		m_CurrentPhaseVM.Value = phaseVM;
	}

	protected override void OnDispose()
	{
		ClearPhaseSubscription();
		ClearPortrait();
		HideVisualSettings();
		foreach (var phases in m_PhasesList)
		{
			phases.Item2.Dispose();
		}
		m_PhasesList.Clear();
		m_PlaningLevelUpManager?.Dispose();
		m_PlaningLevelUpManager = null;
		SoundState.Instance.OnMusicChargenStateChange(MusicStateHandler.MusicChargenState.None);
		base.OnDispose();
	}

	private void UpdateState()
	{
		if (m_IsPhasesDirty)
		{
			UpdatePhases();
		}
	}

	public void Complete()
	{
		UISounds.Instance.Play(ButtonsSounds.Instance.FinishChargenButton.Click, isButton: false, playAnyway: true);
		bool syncPortrait = PhotonManager.Lobby.IsActive && PhotonManager.Initialized && PhotonManager.Instance.PortraitSyncer.IsNeedSyncPortrait();
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: true, syncPortrait);
		Metrics.Chargen.Finish().Send();
	}

	public void Close()
	{
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: false, syncPortrait: false);
	}

	async void ICharGenCloseHandler.HandleClose(bool withComplete, bool syncPortrait)
	{
		if (syncPortrait)
		{
			if (!m_PortraitSynchronizingInProgress)
			{
				m_PortraitSynchronizingInProgress = true;
				try
				{
					await PhotonManager.Instance.PortraitSyncer.SyncPortraits(UtilityNet.IsControlMainCharacter());
				}
				finally
				{
					m_PortraitSynchronizingInProgress = false;
				}
				if (UtilityNet.IsControlMainCharacter())
				{
					Game.Instance.GameCommandQueue.CharGenClose(withComplete, syncPortrait: false);
				}
			}
		}
		else if (withComplete)
		{
			CloseWithComplete();
		}
		else
		{
			CloseWithoutComplete();
		}
	}

	private void CloseWithComplete()
	{
		CharGenContext.EnsureDollSet();
		CharGenContext.CommitLevelUp();
		m_CompleteAction?.Invoke(CharGenContext.CurrentUnit.CurrentValue);
		if (CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)CharGenContext.CurrentUnit.CurrentValue, (Action<ILevelUpCompleteUIHandler>)delegate(ILevelUpCompleteUIHandler h)
			{
				h.HandleLevelUpComplete();
			}, isCheckRuntime: true);
		}
	}

	private void CloseWithoutComplete()
	{
		m_CloseAction?.Invoke();
	}

	public void SetLastPhase(CharGenPhaseBaseVM viewModel)
	{
		m_LastPhase = viewModel;
	}

	public void SwitchVisualSettings()
	{
		if (VisualSettingsVM.CurrentValue == null)
		{
			ShowVisualSettings();
		}
		else
		{
			HideVisualSettings();
		}
	}

	public void ShowVisualSettings()
	{
		if (VisualSettingsVM.CurrentValue == null)
		{
			m_VisualSettingsVM.Value = new CharacterVisualSettingsVM(CharGenContext.Doll, HideVisualSettings).AddTo(this);
		}
	}

	public void HideVisualSettings()
	{
		VisualSettingsVM.Dispose();
		m_VisualSettingsVM.Value = null;
	}

	public void ToCharInfo()
	{
		if (m_CurrentPhaseVM.Value != null)
		{
			CharInfoPageType pageType = m_CurrentPhaseVM.Value.PhaseType switch
			{
				CharGenPhaseType.LevelUpUpgrade => CharInfoPageType.Abilities, 
				CharGenPhaseType.LevelUpAbility => CharInfoPageType.Abilities, 
				CharGenPhaseType.LevelUpModification => CharInfoPageType.Abilities, 
				CharGenPhaseType.LevelUpSpecialization => CharInfoPageType.Archetype, 
				CharGenPhaseType.LevelUpTalent => CharInfoPageType.Archetype, 
				_ => CharInfoPageType.Characteristics, 
			};
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenCharacterInfo(pageType, CharGenContext.CurrentUnit.CurrentValue);
			});
			m_IsInCharscreen.Value = true;
		}
	}

	public void TogglePlaningMode()
	{
		m_IsPlaningMode = !m_IsPlaningMode;
		m_IsPhasesDirty = true;
	}

	private void HandleUpdatePhases()
	{
		m_IsPhasesDirty = true;
		HideVisualSettings();
	}

	private void UpdatePhases()
	{
		List<(SelectionState, CharGenPhaseBaseVM)> list = new List<(SelectionState, CharGenPhaseBaseVM)>();
		AddChargenPhases(list);
		AddLevelUpPhases(list);
		m_PhasesList.Clear();
		m_PhasesList.AddRange(list);
		UpdatePhasesLinks();
		m_IsPhasesDirty = false;
	}

	private void AddChargenPhases(List<(SelectionState s, CharGenPhaseBaseVM)> list)
	{
		if (!m_IsLevelUpMode)
		{
			List<(SelectionState, BlueprintSelectionWithUI)> list2 = (from s in CharGenContext.LevelUpManager.CurrentValue.Selections
				where s.PathRank <= 1 && s.Blueprint is BlueprintSelectionWithUI
				select (s: s, (BlueprintSelectionWithUI)s.Blueprint)).ToList();
			for (int i = 0; i < list2.Count; i++)
			{
				AddPhaseFromSelection(list, list2[i], 0);
			}
		}
	}

	private void AddLevelUpPhases(List<(SelectionState s, CharGenPhaseBaseVM)> list)
	{
		if (!m_IsLevelUpMode && !m_IsPlaningMode)
		{
			return;
		}
		(int, int) tuple = CharGenContext.LevelUpManager.CurrentValue.RanksRange;
		if (m_IsPlaningMode && m_PlaningLevelUpManager != null)
		{
			tuple = (1, m_PlaningLevelUpManager.Selections.Max((SelectionState s) => s.PathRank));
		}
		if (tuple.Item1 > tuple.Item2)
		{
			Close();
		}
		var (i, _) = tuple;
		for (; i <= tuple.Item2; i++)
		{
			AddPhaseByRank(list, i);
		}
	}

	private void AddPhaseByRank(List<(SelectionState s, CharGenPhaseBaseVM)> list, int rank)
	{
		LevelUpManager obj = ((m_IsPlaningMode && m_PlaningLevelUpManager != null && rank > 1) ? m_PlaningLevelUpManager : CharGenContext.LevelUpManager.CurrentValue);
		List<BlueprintFeature> list2 = (from f in obj.Features
			where f.PathRank == rank
			select f.Item3).ToList();
		List<(SelectionState s, BlueprintSelectionWithUI)> list3 = (from s in obj.Selections
			where s.PathRank == rank && s.Blueprint is BlueprintSelectionWithUI
			select (s: s, (BlueprintSelectionWithUI)s.Blueprint)).ToList();
		if (list2.Count > 0)
		{
			AddPhase(list, null, new CharGenLevelUpFeaturePhaseVM(list2, CharGenContext, LevelUpInfoSectionVM, rank));
			rank = -Math.Abs(rank);
		}
		foreach (var item in list3)
		{
			AddPhaseFromSelection(list, item, rank);
			rank = -Math.Abs(rank);
		}
	}

	private void AddPhaseFromSelection(List<(SelectionState s, CharGenPhaseBaseVM)> list, (SelectionState, BlueprintSelectionWithUI) selection, int rank)
	{
		int num = m_PhasesList.FindIndex(((SelectionState, CharGenPhaseBaseVM) p) => p.Item1 == selection.Item1);
		if (num >= 0 && CheckPrerequisites(selection.Item1))
		{
			list.Add(m_PhasesList[num]);
		}
		else if (CheckPrerequisites(selection.Item1))
		{
			CharGenPhaseBaseVM phase = GetPhase(selection.Item1, selection.Item2, rank);
			if (phase != null)
			{
				AddPhase(list, selection.Item1, phase);
			}
		}
	}

	private CharGenPhaseBaseVM GetPhase(SelectionState selectionState, BlueprintSelectionWithUI blueprintSelection, int rank = 0)
	{
		return blueprintSelection.PhaseType switch
		{
			SelectionUIPhaseType.Appearance => new CharGenAppearanceComponentAppearancePhaseVM(CharGenContext, (SelectionStateGender)selectionState), 
			SelectionUIPhaseType.Portrait => new CharGenPortraitPhaseVM(CharGenContext, (SelectionStatePortrait)selectionState, (BlueprintPortraitSelection)blueprintSelection), 
			SelectionUIPhaseType.Homeworld => new CharGenHomeworldPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, ChargenInfoSectionVM), 
			SelectionUIPhaseType.DeathWorld => new CharGenDeathWorldFeaturePhaseVM(CharGenContext, (SelectionStateFeature)selectionState, ChargenInfoSectionVM), 
			SelectionUIPhaseType.Occupation => new CharGenOccupationPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, ChargenInfoSectionVM), 
			SelectionUIPhaseType.Noble => new CharGenNobleHomeworldChildPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, ChargenInfoSectionVM), 
			SelectionUIPhaseType.Voice => new CharGenVoicePhaseVM(CharGenContext, (SelectionStateVoice)selectionState, (BlueprintVoiceSelection)blueprintSelection), 
			SelectionUIPhaseType.Name => new CharGenSummaryPhaseVM(CharGenContext, (SelectionStateCharacterName)selectionState), 
			SelectionUIPhaseType.Career => new CharGenCareerPhaseVM(CharGenContext, ChargenInfoSectionVM, (SelectionStateFeature)selectionState), 
			SelectionUIPhaseType.Specialization => new CharGenLevelUpSpecializationPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, rank), 
			SelectionUIPhaseType.Attributes => new CharGenLevelUpCharacteristicsPhaseVM(CharGenContext, (SelectionStateStats)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, PartyStatsOverviewVM, rank), 
			SelectionUIPhaseType.Skill => new CharGenLevelUpSkillPhaseVM(CharGenContext, (SelectionStateStats)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, PartyStatsOverviewVM, rank), 
			SelectionUIPhaseType.Upgrade => new CharGenLevelUpAbilityUpgradePhaseVM(CharGenContext, (SelectionStateFeature)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, rank), 
			SelectionUIPhaseType.Ability => new CharGenLevelUpAbilityPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, rank), 
			SelectionUIPhaseType.Modification => new CharGenLevelUpModificationPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, rank), 
			SelectionUIPhaseType.Talent => new CharGenLevelUpTalentPhaseVM(CharGenContext, (SelectionStateFeature)selectionState, m_IsLevelUpMode ? LevelUpInfoSectionVM : ChargenInfoSectionVM, rank), 
			_ => null, 
		};
	}

	private void AddPhase(List<(SelectionState, CharGenPhaseBaseVM)> list, SelectionState state, CharGenPhaseBaseVM phase)
	{
		if (phase != null)
		{
			phase.AddTo(this);
			list.Add((state, phase));
		}
	}

	private void UpdatePhasesLinks()
	{
		if (!m_PhasesList.Any())
		{
			return;
		}
		List<CharGenPhaseBaseVM> list = m_PhasesCollection.ToList();
		for (int i = 0; i < m_PhasesList.Count; i++)
		{
			CharGenPhaseBaseVM item = m_PhasesList[i].Item2;
			if (m_PhasesCollection.Contains(item))
			{
				m_PhasesCollection.Move(m_PhasesCollection.IndexOf(item), i);
				list.Remove(item);
			}
			else
			{
				m_PhasesCollection.Insert(i, item);
			}
		}
		foreach (CharGenPhaseBaseVM item2 in list)
		{
			item2.Dispose();
			m_PhasesCollection.Remove(item2);
		}
		m_PhasesCollection[0].UpdateAvailableState(previousIsCompleted: true);
		for (int j = 0; j < m_PhasesCollection.Count - 1; j++)
		{
			m_PhasesCollection[j].SetNextPhase(m_PhasesCollection[j + 1]);
		}
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			m_Unit.Value = CharGenContext.LevelUpManager.CurrentValue.PreviewUnit;
			ClearPortrait();
			m_PortraitSubscription = CharGenContext.Doll.GetReactiveProperty((DollState dollState) => dollState.PortraitData).Subscribe(delegate(PortraitData value)
			{
				PortraitVM.CurrentValue?.Dispose();
				PortraitData portraitData = value ?? manager.PreviewUnit?.Portrait;
				m_PortraitVM.Value = new PortraitVM(portraitData);
			});
		}
	}

	private void ClearPortrait()
	{
		m_PortraitSubscription?.Dispose();
		m_PortraitSubscription = null;
		PortraitVM.CurrentValue?.Dispose();
		m_PortraitVM.Value = null;
	}

	private void ClearPhaseSubscription()
	{
		m_PhaseSubscriptions.Clear();
		m_CurrentPhaseIsCompleted.Value = false;
		m_NextButtonHint.Value = string.Empty;
	}

	private void UpdateChargenMusicState(CharGenPhaseBaseVM phase)
	{
		SoundState.Instance.OnMusicChargenStateChange(phase.ChargenMusicState);
	}

	private bool CheckPrerequisites(SelectionState state)
	{
		if (!(state is SelectionStateFeature selectionStateFeature))
		{
			return true;
		}
		if (!CharGenContext.IsCustomCharacter.CurrentValue)
		{
			return false;
		}
		LevelUpManager levelUpManager = ((m_IsPlaningMode && m_PlaningLevelUpManager != null && state.PathRank > 1) ? m_PlaningLevelUpManager : CharGenContext.LevelUpManager.CurrentValue);
		if (levelUpManager != null)
		{
			return selectionStateFeature.Blueprint.MeetPrerequisites(levelUpManager.PreviewUnit);
		}
		return false;
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
		ClearPhasesAfterCareerChanged();
		UpdatePhases();
	}

	private void ClearPhasesAfterCareerChanged()
	{
		if (!CharGenContext.LevelUpManager.CurrentValue.PreviewUnit.Progression.AllCareerPaths.Empty())
		{
			BlueprintCareerPath item = CharGenContext.LevelUpManager.CurrentValue.PreviewUnit.Progression.AllCareerPaths.FirstOrDefault().Blueprint;
			if (item != null)
			{
				m_PlaningLevelUpManager?.Dispose();
				m_PlaningLevelUpManager = new LevelUpManager(CharGenContext.LevelUpManager.CurrentValue.PreviewUnit, item, autoCommit: false, item.RankEntries.Count());
				m_IsPhasesDirty = true;
			}
		}
	}

	public void HandleUICommitChanges()
	{
	}

	public void HandleUISelectionChanged()
	{
		(int, int) ranksRange = CharGenContext.LevelUpManager.CurrentValue.RanksRange;
		int num = ranksRange.Item1 - 1;
		var (i, _) = ranksRange;
		for (; i <= ranksRange.Item2; i++)
		{
			num = ((from p in m_PhasesList
				select p.Item2 into p
				where Math.Abs(p.Rank) == i
				select p).All((CharGenPhaseBaseVM p) => p.IsCompletedAndAvailable.CurrentValue) ? i : num);
		}
		if (CharGenContext.CharGenConfig.Mode != CharGenMode.LevelUp)
		{
			m_IsPhasesDirty = true;
		}
		ProgressionVM?.SetLastFinishedRank(num);
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		m_CheckCoopControls.Execute(UtilityNet.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}

	public SelectionStateStats GetAttributeSelectionIfAvailable()
	{
		CharGenContext charGenContext = CharGenContext;
		if (charGenContext != null)
		{
			ReadOnlyReactiveProperty<LevelUpManager> levelUpManager = charGenContext.LevelUpManager;
			if (levelUpManager != null)
			{
				LevelUpManager currentValue = levelUpManager.CurrentValue;
				if (currentValue != null)
				{
					_ = currentValue.Selections;
					if (0 == 0)
					{
						foreach (SelectionState selection in CharGenContext.LevelUpManager.CurrentValue.Selections)
						{
							if (selection is SelectionStateStats selectionStateStats && selectionStateStats.Blueprint is BlueprintSelectionAttributes)
							{
								return selectionStateStats;
							}
						}
						return null;
					}
				}
			}
		}
		return null;
	}
}
