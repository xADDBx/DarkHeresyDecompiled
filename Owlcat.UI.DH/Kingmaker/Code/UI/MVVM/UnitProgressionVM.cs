using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitProgressionVM : BaseUnitProgressionVM
{
	private readonly ReactiveProperty<UnitProgressionWindowState> m_State = new ReactiveProperty<UnitProgressionWindowState>(UnitProgressionWindowState.CareerPathList);

	private readonly ReactiveProperty<CareerPathVM> m_PreselectedCareer = new ReactiveProperty<CareerPathVM>();

	public readonly AutoDisposingList<CareerPathsListVM> CareerPathsList = new AutoDisposingList<CareerPathsListVM>();

	public readonly ObservableList<ProgressionBreadcrumbsItemVM> Breadcrumbs = new ObservableList<ProgressionBreadcrumbsItemVM>();

	private readonly ObservableList<CareerPathVM> m_AllCareerPaths = new ObservableList<CareerPathVM>();

	private IDisposable m_EscHandle;

	private bool m_RankEntryHasSelection;

	private readonly UnitProgressionMode m_ProgressionMode;

	public readonly CharInfoExperienceVM CharInfoExperienceVM;

	public readonly UnitBackgroundBlockVM UnitBackgroundBlockVM;

	public ReadOnlyReactiveProperty<UnitProgressionWindowState> State => m_State;

	public ReadOnlyReactiveProperty<CareerPathVM> PreselectedCareer => m_PreselectedCareer;

	public IEnumerable<CareerPathVM> AllCareerPaths => m_AllCareerPaths;

	public bool IsCharGen => m_ProgressionMode == UnitProgressionMode.CharGen;

	public UnitProgressionVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReactiveProperty<LevelUpManager> levelUpManager, UnitProgressionMode mode)
		: base(unit, levelUpManager)
	{
		m_ProgressionMode = mode;
		CharInfoExperienceVM = new CharInfoExperienceVM(unit).AddTo(this);
		UnitBackgroundBlockVM = new UnitBackgroundBlockVM(unit).AddTo(this);
		TryRestoreSavedState();
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	public void SetCareerPath(CareerPathVM careerPathVM)
	{
		m_PreselectedCareer.Value = careerPathVM;
	}

	private void DisposeImplementation()
	{
		TrySaveState();
		DestroyLevelUpManager();
		Clear();
		m_EscHandle?.Dispose();
		m_EscHandle = null;
	}

	protected override void RefreshData()
	{
		Clear();
		if (Unit.CurrentValue == null)
		{
			return;
		}
		Dictionary<CareerPathTier, List<CareerPathVM>> dictionary = new Dictionary<CareerPathTier, List<CareerPathVM>>();
		foreach (BlueprintCareerPath careerPath in ProgressionRoot.Instance.CareerPaths)
		{
			if (!dictionary.ContainsKey(careerPath.Tier))
			{
				dictionary[careerPath.Tier] = new List<CareerPathVM>();
			}
			try
			{
				CareerPathVM item = new CareerPathVM(Unit.CurrentValue, careerPath, this);
				dictionary[careerPath.Tier].Add(item);
				m_AllCareerPaths.Add(item);
			}
			catch (Exception ex)
			{
				PFLog.UI.Exception(ex);
			}
		}
		List<BlueprintCareerPath> list = Unit.CurrentValue.Facts.List.Select((EntityFact f) => f.Blueprint as BlueprintCareerPath).ToList();
		list.RemoveAll((BlueprintCareerPath c) => c == null);
		foreach (CareerPathTier value in Enum.GetValues(typeof(CareerPathTier)))
		{
			if (dictionary.ContainsKey(value))
			{
				CareerPathsListVM item2 = new CareerPathsListVM(value, dictionary[value], PreselectedCareer, list).AddTo(this);
				CareerPathsList.Add(item2);
			}
		}
		m_CurrentRankEntryItem.Value = null;
		SetCareerPath(TryGetActiveLevelupCareer(), force: true);
		TryGetActiveLevelupCareer()?.UpdateCareerPath();
	}

	public override void HandleUICommitChanges()
	{
	}

	public override void SetRankEntry(IRankEntrySelectItem rankEntryItem)
	{
		if (rankEntryItem != null && rankEntryItem == base.CurrentRankEntryItem.CurrentValue)
		{
			m_OnRepeatedCurrentRankEntryItem.Execute(rankEntryItem);
		}
		m_CurrentRankEntryItem.Value = rankEntryItem;
	}

	public override void SetCareerPath(CareerPathVM careerPathVM, bool force = false)
	{
		careerPathVM?.InitializeRankEntries();
		CareerPathVM oldCareer = (force ? null : base.CurrentCareer.CurrentValue);
		TrySelectCareerPath(oldCareer, careerPathVM);
		UpdateState();
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(SetFirstAvailableRankEntry);
	}

	public void SetFirstAvailableRankEntry()
	{
		if (base.CurrentCareer.CurrentValue == null)
		{
			return;
		}
		RankEntrySelectionVM rankEntrySelectionVM = base.CurrentCareer.CurrentValue.AllSelections?.LastOrDefault((RankEntrySelectionVM s) => s.SelectionMade);
		if (base.CurrentCareer.CurrentValue.IsInLevelupProcess)
		{
			SetRankEntry(rankEntrySelectionVM);
			if (base.CurrentCareer.CurrentValue.LastEntryToUpgrade == rankEntrySelectionVM)
			{
				base.CurrentCareer.CurrentValue.SetRankEntry(rankEntrySelectionVM);
			}
			else
			{
				base.CurrentCareer.CurrentValue.SelectNextItem(skipSelected: false);
			}
		}
		else
		{
			base.CurrentCareer.CurrentValue.SetRankEntry(null);
		}
	}

	public override void Commit()
	{
		Game.Instance.GameCommandQueue.CommitLvlUp(base.LevelUpManager);
		DestroyLevelUpManager();
		RefreshData();
	}

	public void UpdateSelectionsFromUnit(BaseUnitEntity unit)
	{
		UnitBackgroundBlockVM.UpdateSelectionsFromUnit(unit);
	}

	private void TrySelectCareerPath(CareerPathVM oldCareer, CareerPathVM newCareer)
	{
		if (m_ProgressionMode == UnitProgressionMode.CharGen || (base.LevelUpManager != null && base.LevelUpManager.TargetUnit != Unit.CurrentValue))
		{
			m_CurrentCareer.Value = newCareer;
			ReactiveProperty<IRankEntrySelectItem> firstAvailableEntryItem = m_FirstAvailableEntryItem;
			CareerPathVM careerPathVM = newCareer;
			firstAvailableEntryItem.Value = ((careerPathVM == null || careerPathVM.CareerPath.Tier != 0) ? null : base.CurrentCareer.CurrentValue?.RankEntries.FirstOrDefault()?.GetFirstItem());
			return;
		}
		if (oldCareer != null)
		{
			if (oldCareer.AvailableSelections.Select((RankEntrySelectionVM i) => i.SelectedFeature.CurrentValue).Any((RankEntrySelectionFeatureVM i) => i != null))
			{
				string message = ((m_ProgressionMode == UnitProgressionMode.CharGen) ? UIStrings.Instance.CharacterSheet.DialogCloseProgression : UIStrings.Instance.CharacterSheet.LevelupDialogCloseProgression);
				CloseWithMessage(message, OnYes, null);
			}
			else
			{
				DestroyLevelUpManager();
				m_CurrentCareer.Value = newCareer;
			}
		}
		else
		{
			if (newCareer != null && newCareer.IsAvailableToUpgrade)
			{
				CreateLevelUpManager(newCareer.CareerPath);
				EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
				{
					h.HandleUISelectCareerPath();
				});
			}
			m_CurrentCareer.Value = newCareer;
			base.CurrentCareer.CurrentValue?.CurrentProgress.Subscribe(delegate
			{
				m_FirstAvailableEntryItem.Value = base.CurrentCareer.CurrentValue?.FirstSelectable;
			}).AddTo(this);
		}
		TrySaveState();
		void OnYes()
		{
			DestroyLevelUpManager();
			m_CurrentCareer.Value = newCareer;
			UpdateState();
		}
	}

	public void TryClose(Action onYes, Action onNo)
	{
		CareerPathVM currentValue = base.CurrentCareer.CurrentValue;
		if (currentValue != null && currentValue.AvailableSelections.Select((RankEntrySelectionVM i) => i.SelectedFeature.CurrentValue).Any((RankEntrySelectionFeatureVM i) => i != null))
		{
			CloseWithMessage(UIStrings.Instance.CharacterSheet.LevelupDialogCloseProgression, onYes, onNo);
		}
		else
		{
			onYes?.Invoke();
		}
	}

	private void CloseWithMessage(string message, Action onYes, Action onNo)
	{
		UtilityMessageBox.ShowMessageBox(message, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes)
			{
				onYes?.Invoke();
			}
			else
			{
				onNo?.Invoke();
			}
		});
	}

	private CareerPathVM TryGetActiveLevelupCareer()
	{
		if (base.LevelUpManager == null || base.LevelUpManager.TargetUnit != Unit.CurrentValue)
		{
			return null;
		}
		return GetCareerPathByBlueprint(base.LevelUpManager.Path);
	}

	private CareerPathVM GetCareerPathByBlueprint(BlueprintPath path)
	{
		return CareerPathsList.SelectMany((CareerPathsListVM i) => i.CareerPathVMs).FirstOrDefault((CareerPathVM i) => i.CareerPath == path);
	}

	private void CreateLevelUpManager(BlueprintCareerPath careerPath)
	{
		LevelUpManager manager = new LevelUpManager(Unit.CurrentValue, careerPath, autoCommit: false);
		m_LevelUpManager.Value = manager;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleCreateLevelUpManager(manager);
		});
	}

	public void ClearLevelupManagerIfNeeded(BaseUnitEntity newUnitEntity)
	{
		if (base.LevelUpManager != null && base.LevelUpManager.TargetUnit != newUnitEntity && newUnitEntity != null)
		{
			DestroyLevelUpManager();
		}
	}

	private void DestroyLevelUpManager()
	{
		m_LevelUpManager.Value?.Dispose();
		m_LevelUpManager.Value = null;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleDestroyLevelUpManager();
		});
	}

	private void TryRestoreSavedState()
	{
		if (m_ProgressionMode == UnitProgressionMode.CharGen)
		{
			return;
		}
		SavedUnitProgressionWindowData savedData = Game.Instance.Player.UISettings.SavedUnitProgressionWindowData;
		CareerPathVM careerPathVM = null;
		if (savedData.CareerPath == null)
		{
			return;
		}
		foreach (CareerPathsListVM careerPaths in CareerPathsList)
		{
			careerPathVM = careerPaths.CareerPathVMs.FirstOrDefault((CareerPathVM careerPath) => careerPath.CareerPath == savedData.CareerPath.Get());
			if (careerPathVM != null)
			{
				break;
			}
		}
		careerPathVM?.SetCareerPath();
	}

	private void TrySaveState()
	{
		PlayerUISettings uISettings = Game.Instance.Player.UISettings;
		if (m_ProgressionMode != 0)
		{
			CareerPathVM currentValue = base.CurrentCareer.CurrentValue;
			if (currentValue != null && currentValue.IsUnlocked)
			{
				uISettings.SavedUnitProgressionWindowData.CareerPath = base.CurrentCareer.CurrentValue?.CareerPath.ToReference<BlueprintCareerPath.Reference>();
				return;
			}
		}
		uISettings.SavedUnitProgressionWindowData.CareerPath = null;
	}

	private void SetState(UnitProgressionWindowState newState, bool saveSelections = false)
	{
		switch (newState)
		{
		case UnitProgressionWindowState.CareerPathList:
			SetCareerPath(null);
			break;
		}
		UpdateState();
	}

	public void SetPreviousState(bool saveSelections = false)
	{
		int num = Breadcrumbs.Count - 1;
		int index = Math.Max(0, num - 1);
		SetState(Breadcrumbs[index].ProgressionState, saveSelections);
	}

	private void UpdateState()
	{
		foreach (CareerPathsListVM careerPaths in CareerPathsList)
		{
			careerPaths.UpdateState();
		}
		UnitProgressionWindowState unitProgressionWindowState = ((base.CurrentCareer.CurrentValue != null) ? UnitProgressionWindowState.CareerPathProgression : UnitProgressionWindowState.CareerPathList);
		UpdateBreadcrumbs(unitProgressionWindowState);
		m_State.Value = unitProgressionWindowState;
	}

	private void UpdateBreadcrumbs(UnitProgressionWindowState newState)
	{
		Breadcrumbs.Clear();
		foreach (UnitProgressionWindowState value in Enum.GetValues(typeof(UnitProgressionWindowState)))
		{
			bool flag = newState == value;
			string text = value switch
			{
				UnitProgressionWindowState.CareerPathList => UIStrings.Instance.CharacterSheet.GetMenuLabel(CharInfoPageType.LevelProgression), 
				UnitProgressionWindowState.CareerPathProgression => base.CurrentCareer.CurrentValue?.Name, 
				_ => string.Empty, 
			};
			Breadcrumbs.Add(new ProgressionBreadcrumbsItemVM(value, text, flag, delegate(UnitProgressionWindowState s)
			{
				SetState(s);
			}));
			if (flag)
			{
				break;
			}
		}
		m_EscHandle?.Dispose();
	}

	private void Clear()
	{
		foreach (CareerPathVM allCareerPath in m_AllCareerPaths)
		{
			allCareerPath.Dispose();
		}
		m_AllCareerPaths.Clear();
		CareerPathsList.Clear();
		Breadcrumbs.Clear();
	}
}
