using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code.View.UI.MVVM;
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
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using Photon.Realtime;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenVM : ViewModel, ILevelUpManagerUIHandler, ISubscriber, ICharGenCloseHandler, INetLobbyPlayersHandler
{
	public readonly CharGenContext CharGenContext;

	private readonly List<CharGenPhaseBaseVM> m_PhasesList = new List<CharGenPhaseBaseVM>();

	public readonly SelectionGroupRadioVM<CharGenPhaseBaseVM> PhasesSelectionGroupRadioVM;

	private readonly ReactiveProperty<CharGenPhaseBaseVM> m_CurrentPhaseVM = new ReactiveProperty<CharGenPhaseBaseVM>();

	private readonly ReactiveProperty<PortraitVM> m_PortraitVM = new ReactiveProperty<PortraitVM>();

	private readonly ReactiveProperty<CharacterVisualSettingsVM> m_VisualSettingsVM = new ReactiveProperty<CharacterVisualSettingsVM>();

	private readonly ReactiveProperty<bool> m_CurrentPhaseIsCompleted = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShouldShowVisualSettings = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsInCharscreen = new ReactiveProperty<bool>();

	private readonly ObservableList<CharGenPhaseBaseVM> m_PhasesCollection = new ObservableList<CharGenPhaseBaseVM>();

	private CharGenPhaseBaseVM m_LastPhase;

	private readonly Action m_CloseAction;

	private readonly Action<BaseUnitEntity> m_CompleteAction;

	private IDisposable m_PortraitSubscription;

	private IDisposable m_PhaseIsCompletedSubscription;

	private readonly CompositeDisposable m_PhaseSubscriptions = new CompositeDisposable();

	private bool m_IsPhasesDirty;

	private readonly bool m_IsCustomCompanionChargen;

	private readonly ReactiveCommand<bool> m_CheckCoopControls = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<bool> m_IsMainCharacter = new ReactiveProperty<bool>();

	private bool m_PortraitSynchronizingInProgress;

	public ObservableList<CharGenPhaseBaseVM> PhasesCollection => m_PhasesCollection;

	public ReadOnlyReactiveProperty<CharGenPhaseBaseVM> CurrentPhaseVM => m_CurrentPhaseVM;

	public ReadOnlyReactiveProperty<PortraitVM> PortraitVM => m_PortraitVM;

	public ReadOnlyReactiveProperty<CharacterVisualSettingsVM> VisualSettingsVM => m_VisualSettingsVM;

	public ReadOnlyReactiveProperty<bool> CurrentPhaseIsCompleted => m_CurrentPhaseIsCompleted;

	public ReadOnlyReactiveProperty<bool> ShouldShowVisualSettings => m_ShouldShowVisualSettings;

	public ReadOnlyReactiveProperty<bool> IsInCharscreen => m_IsInCharscreen;

	public bool CurrentPhaseCanInterrupt => CurrentPhaseVM.CurrentValue?.CanInterruptChargen ?? false;

	public CharGenPhaseBaseVM LastPhase => m_LastPhase;

	public InfoSectionVM InfoSectionVM { get; private set; }

	public ChargenProgressionVM ProgressionVM { get; private set; }

	public Observable<bool> CheckCoopControls => m_CheckCoopControls;

	public ReadOnlyReactiveProperty<bool> IsMainCharacter => m_IsMainCharacter;

	public CharGenVM(CharGenConfig config, Action closeAction, Action<BaseUnitEntity> completeAction, bool isCustomCompanionChargen)
	{
		m_CloseAction = closeAction;
		m_CompleteAction = completeAction;
		m_IsCustomCompanionChargen = isCustomCompanionChargen;
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		InfoSectionVM = new InfoSectionVM().AddTo(this);
		CharGenContext = new CharGenContext(config).AddTo(this);
		CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager).AddTo(this);
		CharGenContext.SetPregenUnit(config.Unit);
		CharGenContext.IsCustomCharacter.Subscribe(delegate
		{
			m_IsPhasesDirty = true;
			HideVisualSettings();
		}).AddTo(this);
		ProgressionVM = new ChargenProgressionVM(CharGenContext.LevelUpManager, m_CurrentPhaseVM).AddTo(this);
		UpdatePhases();
		PhasesSelectionGroupRadioVM = new SelectionGroupRadioVM<CharGenPhaseBaseVM>(m_PhasesCollection, m_CurrentPhaseVM).AddTo(this);
		PhasesSelectionGroupRadioVM.TrySelectFirstValidEntity();
		CurrentPhaseVM.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(CharGenPhaseBaseVM phase)
		{
			ClearPhaseSubscription();
			HideVisualSettings();
			if (phase != null)
			{
				m_PhaseSubscriptions.Add(phase.IsCompletedAndAvailable.Subscribe(delegate(bool value)
				{
					m_CurrentPhaseIsCompleted.Value = value;
				}));
				m_PhaseSubscriptions.Add(phase.ShowVisualSettings.Subscribe(delegate(bool value)
				{
					m_ShouldShowVisualSettings.Value = value;
				}));
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
	}

	public void SetCurrentPhase(CharGenPhaseBaseVM phaseVM)
	{
		m_CurrentPhaseVM.Value = phaseVM;
	}

	public void SetInputLayer(InputLayer inputLayer, Action<InputLayer, ReactiveProperty<bool>> setter)
	{
		setter(inputLayer, m_IsMainCharacter);
	}

	protected override void OnDispose()
	{
		ClearPhaseSubscription();
		ClearPortrait();
		HideVisualSettings();
		foreach (CharGenPhaseBaseVM phases in m_PhasesList)
		{
			phases.Dispose();
		}
		m_PhasesList.Clear();
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
		UISounds.Instance.Play(UISounds.Instance.Sounds.Buttons.FinishChargenButtonClick, isButton: false, playAnyway: true);
		bool syncPortrait = PhotonManager.Lobby.IsActive && PhotonManager.Initialized && PhotonManager.Instance.PortraitSyncer.IsNeedSyncPortrait();
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: true, syncPortrait);
	}

	public void Close()
	{
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: false, syncPortrait: false);
	}

	async void ICharGenCloseHandler.HandleClose(bool withComplete, bool syncPortrait)
	{
		switch (CharGenContext.CharGenConfig.Mode)
		{
		case CharGenMode.LevelUp:
			Metrics.Interface.InterfaceType(InterfaceMetricsEvent.InterfaceTypes.LevelUp).InterfaceState(InterfaceMetricsEvent.InterfaceStates.Close).Send();
			break;
		case CharGenMode.NewGame:
		case CharGenMode.NewCompanion:
			Metrics.Interface.InterfaceType(InterfaceMetricsEvent.InterfaceTypes.CharGen).InterfaceState(InterfaceMetricsEvent.InterfaceStates.Close).Send();
			return;
		}
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
		if (m_CurrentPhaseVM.Value == null)
		{
			return;
		}
		switch (m_CurrentPhaseVM.Value.PhaseType)
		{
		case CharGenPhaseType.LevelUpAbility:
		case CharGenPhaseType.LevelUpModification:
		case CharGenPhaseType.LevelUpUpgrade:
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenCharacterInfo(CharInfoPageType.Abilities, CharGenContext.CurrentUnit.CurrentValue);
			});
			break;
		case CharGenPhaseType.LevelUpSpecialization:
		case CharGenPhaseType.LevelUpTalent:
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenCharacterInfo(CharInfoPageType.Archetype, CharGenContext.CurrentUnit.CurrentValue);
			});
			break;
		default:
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenCharacterInfo(CharInfoPageType.Characteristics, CharGenContext.CurrentUnit.CurrentValue);
			});
			break;
		}
		m_IsInCharscreen.Value = true;
	}

	private void HandleUpdatePhases()
	{
		m_IsPhasesDirty = true;
		HideVisualSettings();
	}

	private void UpdatePhases()
	{
		List<CharGenPhaseBaseVM> phasesList = m_PhasesList;
		bool currentValue = CharGenContext.IsCustomCharacter.CurrentValue;
		bool flag = CharGenContext.CharGenConfig.Mode == CharGenMode.NewGame;
		if (CharGenContext.LevelUpManager.CurrentValue.Path == null)
		{
			AddPhase(phasesList, new CharGenCareerPhaseVM_NEW(CharGenContext));
			UpdatePhasesLinks();
			m_IsPhasesDirty = false;
			return;
		}
		if (CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp)
		{
			(int, int) ranksRange = CharGenContext.LevelUpManager.CurrentValue.RanksRange;
			if (ranksRange.Item1 > ranksRange.Item2)
			{
				Close();
			}
			var (i, _) = ranksRange;
			for (; i <= ranksRange.Item2; i++)
			{
				AddPhaseByRank(i);
			}
			UpdatePhasesLinks();
			m_IsPhasesDirty = false;
			return;
		}
		if (TryClearPhaseFromList<CharGenPregenPhaseVM>(check: true, phasesList))
		{
			AddPhase(phasesList, new CharGenPregenPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenAppearanceComponentAppearancePhaseVM>(check: true, phasesList))
		{
			AddPhase(phasesList, new CharGenAppearanceComponentAppearancePhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenSoulMarkPhaseVM>(currentValue && !flag, phasesList))
		{
			AddPhase(phasesList, new CharGenSoulMarkPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenHomeworldPhaseVM>(currentValue, phasesList))
		{
			AddPhase(phasesList, new CharGenHomeworldPhaseVM(CharGenContext));
		}
		bool check = HasFeatureGroupInSelections(FeatureGroup.ChargenImperialWorld);
		if (TryClearPhaseFromList<CharGenImperialHomeworldChildPhaseVM>(check, phasesList))
		{
			AddPhase(phasesList, new CharGenImperialHomeworldChildPhaseVM(CharGenContext));
		}
		bool check2 = HasFeatureGroupInSelections(FeatureGroup.ChargenForgeWorld);
		if (TryClearPhaseFromList<CharGenForgeHomeworldChildPhaseVM>(check2, phasesList))
		{
			AddPhase(phasesList, new CharGenForgeHomeworldChildPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenOccupationPhaseVM>(currentValue, phasesList))
		{
			AddPhase(phasesList, new CharGenOccupationPhaseVM(CharGenContext));
		}
		bool check3 = HasFeatureGroupInSelections(FeatureGroup.ChargenNavigator);
		if (TryClearPhaseFromList<CharGenNavigatorPhaseVM>(check3, phasesList))
		{
			AddPhase(phasesList, new CharGenNavigatorPhaseVM(CharGenContext));
		}
		bool check4 = HasFeatureGroupInSelections(FeatureGroup.ChargenPsyker);
		if (TryClearPhaseFromList<CharGenSanctionedPsykerChildPhaseVM>(check4, phasesList))
		{
			AddPhase(phasesList, new CharGenSanctionedPsykerChildPhaseVM(CharGenContext));
		}
		bool check5 = HasFeatureGroupInSelections(FeatureGroup.ChargenMomentOfTriumph);
		if (TryClearPhaseFromList<CharGenMomentOfTriumphPhaseVM>(check5, phasesList))
		{
			AddPhase(phasesList, new CharGenMomentOfTriumphPhaseVM(CharGenContext));
		}
		bool check6 = HasFeatureGroupInSelections(FeatureGroup.ChargenDarkestHour);
		if (TryClearPhaseFromList<CharGenDarkestHourPhaseVM>(check6, phasesList))
		{
			AddPhase(phasesList, new CharGenDarkestHourPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenCareerPhaseVM>(currentValue, phasesList))
		{
			AddPhase(phasesList, new CharGenCareerPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenAttributesPhaseVM>(currentValue, phasesList))
		{
			AddPhase(phasesList, new CharGenAttributesPhaseVM(CharGenContext, m_CurrentPhaseVM));
		}
		if (TryClearPhaseFromList<CharGenShipPhaseVM>(flag, phasesList))
		{
			AddPhase(phasesList, new CharGenShipPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenSummaryPhaseVM>(check: true, phasesList))
		{
			AddPhase(phasesList, new CharGenSummaryPhaseVM(CharGenContext));
		}
		UpdatePhasesLinks();
		m_IsPhasesDirty = false;
	}

	private bool TryClearPhaseFromList<TPhase>(bool check, List<CharGenPhaseBaseVM> list) where TPhase : CharGenPhaseBaseVM
	{
		if (check)
		{
			if (!list.Any((CharGenPhaseBaseVM ph) => ph is TPhase))
			{
				return true;
			}
		}
		else
		{
			TPhase val = null;
			foreach (CharGenPhaseBaseVM item in list)
			{
				if (item is TPhase val2)
				{
					val = val2;
				}
			}
			if (val != null && !(CurrentPhaseVM.CurrentValue is TPhase))
			{
				val.Dispose();
				list.Remove(val);
			}
		}
		return false;
	}

	private void AddPhase(List<CharGenPhaseBaseVM> list, CharGenPhaseBaseVM phase)
	{
		phase.AddTo(this);
		list.Add(phase);
	}

	private void AddPhaseByRank(int rank)
	{
		List<BlueprintFeature> list = (from f in CharGenContext.LevelUpManager.CurrentValue.Features
			where f.PathRank == rank
			select f.Item3).ToList();
		if (list.Count > 0)
		{
			AddPhase(m_PhasesList, new CharGenLevelUpFeaturePhaseVM(CharGenContext, list, InfoSectionVM, rank));
			rank = -Math.Abs(rank);
		}
		foreach (SelectionState item in CharGenContext.LevelUpManager.CurrentValue.Selections.Where((SelectionState s) => s.PathRank == rank).ToList())
		{
			if (item is SelectionStateStats selectionStateStats)
			{
				if (selectionStateStats.Blueprint is BlueprintSelectionAttributes)
				{
					AddPhase(m_PhasesList, new CharGenLevelUpCharacteristicsPhaseVM(CharGenContext, selectionStateStats, InfoSectionVM, rank));
				}
				if (selectionStateStats.Blueprint is BlueprintSelectionSkills)
				{
					AddPhase(m_PhasesList, new CharGenLevelUpSkillPhaseVM(CharGenContext, selectionStateStats, InfoSectionVM, rank));
				}
				rank = -Math.Abs(rank);
			}
			if (item is SelectionStateFeature selectionStateFeature)
			{
				switch (selectionStateFeature.Blueprint.Group)
				{
				case FeatureGroup.AbilityUpgrade:
					AddPhase(m_PhasesList, new CharGenLevelUpAbilityUpgradePhaseVM(CharGenContext, selectionStateFeature, InfoSectionVM, rank));
					break;
				case FeatureGroup.ActiveAbility:
					AddPhase(m_PhasesList, new CharGenLevelUpAbilityPhaseVM(CharGenContext, selectionStateFeature, InfoSectionVM, rank));
					break;
				case FeatureGroup.Modifier:
					AddPhase(m_PhasesList, new CharGenLevelUpModificationPhaseVM(CharGenContext, selectionStateFeature, InfoSectionVM, rank));
					break;
				case FeatureGroup.Specialization:
					AddPhase(m_PhasesList, new CharGenLevelUpSpecializationPhaseVM(CharGenContext, selectionStateFeature, InfoSectionVM, rank));
					break;
				case FeatureGroup.Talent:
					AddPhase(m_PhasesList, new CharGenLevelUpTalentPhaseVM(CharGenContext, selectionStateFeature, InfoSectionVM, rank));
					break;
				}
				rank = -Math.Abs(rank);
			}
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
			CharGenPhaseBaseVM item = m_PhasesList[i];
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
	}

	private bool HasFeatureGroupInSelections(FeatureGroup featureGroup)
	{
		if (!CharGenContext.IsCustomCharacter.CurrentValue)
		{
			return false;
		}
		LevelUpManager currentValue = CharGenContext.LevelUpManager.CurrentValue;
		if (currentValue == null)
		{
			return false;
		}
		return UtilityChargen.GetFeatureSelectionsByGroup(currentValue.Path, featureGroup, currentValue.PreviewUnit).Any();
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
		foreach (CharGenPhaseBaseVM item in new List<CharGenPhaseBaseVM>(m_PhasesList))
		{
			if (!(item is CharGenCareerPhaseVM_NEW))
			{
				m_PhasesList.Remove(item);
				m_PhasesCollection.Remove(item);
				item.Dispose();
			}
		}
		m_IsPhasesDirty = true;
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
			num = (m_PhasesList.Where((CharGenPhaseBaseVM p) => Math.Abs(p.Rank) == i).All((CharGenPhaseBaseVM p) => p.IsCompletedAndAvailable.CurrentValue) ? i : num);
		}
		ProgressionVM.SetLastFinishedRank(num);
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
}
