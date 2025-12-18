using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharGenBackgroundBasePhaseVM<TViewModel> : CharGenPhaseBaseVM, ICharGenSelectItemHandler, ISubscriber where TViewModel : CharGenBackgroundBaseItemVM
{
	protected bool CanShowVisualSettings = true;

	private readonly ReactiveProperty<bool> m_CurrentPageIsFirst = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CurrentPageIsLast = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<CharGenPhaseBaseVM> m_CurrentPhase = new ReactiveProperty<CharGenPhaseBaseVM>();

	protected readonly FeatureGroup FeatureGroup;

	protected readonly ObservableList<TViewModel> Items = new ObservableList<TViewModel>();

	private readonly IDelayedSelector m_DelayedApplySelection;

	private BlueprintSelectionFeature m_Selection;

	private SelectionStateFeature m_SelectionStateFeature;

	protected Action OnSelectionApplied;

	protected readonly ReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TViewModel> m_SelectedItem = new ReactiveProperty<TViewModel>();

	public readonly SelectionGroupRadioVM<TViewModel> SelectionGroup;

	protected bool Subscribed;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsFirst => m_CurrentPageIsFirst;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsLast => m_CurrentPageIsLast;

	public ReadOnlyReactiveProperty<TViewModel> SelectedItem => m_SelectedItem;

	protected CharGenBackgroundBasePhaseVM(CharGenContext charGenContext, FeatureGroup featureGroup, CharGenPhaseType phaseType, ReactiveProperty<CharGenPhaseBaseVM> currentPhase = null)
		: base(charGenContext, phaseType)
	{
		IDelayedSelector delayedApplySelection;
		if (!GameUIState.Instance.IsInMainMenu)
		{
			IDelayedSelector delayedSelector = new InGameSelector();
			delayedApplySelection = delayedSelector;
		}
		else
		{
			IDelayedSelector delayedSelector = new MainMenuSelector();
			delayedApplySelection = delayedSelector;
		}
		m_DelayedApplySelection = delayedApplySelection;
		m_CurrentPhase = currentPhase;
		FeatureGroup = featureGroup;
		SelectionGroup = new SelectionGroupRadioVM<TViewModel>(Items, m_SelectedItem);
		AddDisposable(SelectionGroup);
		AddDisposable(SelectedItem.Subscribe(HandleSelectedItem));
		AddDisposable(SelectedItem.Subscribe(delegate(TViewModel value)
		{
			m_CurrentPageIsFirst.Value = Items.FirstOrDefault() == value;
			m_CurrentPageIsLast.Value = Items.LastOrDefault() == value;
		}));
		CreateTooltipSystem();
		TrySelectItem();
		AddDisposable(EventBus.Subscribe(this));
	}

	void ICharGenSelectItemHandler.HandleSelectItem(FeatureGroup featureGroup, BlueprintFeature blueprintFeature)
	{
		if (FeatureGroup != featureGroup)
		{
			return;
		}
		if (!UtilityNet.IsControlMainCharacter())
		{
			TViewModel value = ((blueprintFeature != null) ? Items.FirstOrDefault((TViewModel item) => blueprintFeature == item?.Feature) : null);
			m_SelectedItem.Value = value;
		}
		DelayedApplySelection();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
	}

	protected override bool CheckIsCompleted()
	{
		SelectionStateFeature selectionStateFeature = m_SelectionStateFeature;
		if (selectionStateFeature != null && selectionStateFeature.IsMade)
		{
			return selectionStateFeature.IsValid;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		if (!Subscribed)
		{
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			Subscribed = true;
		}
		else
		{
			RefreshItems(CharGenContext.LevelUpManager.CurrentValue);
			RefreshVisualSettings();
		}
		TrySelectItem();
	}

	protected virtual void Clear()
	{
		if (m_DelayedApplySelection.IsRunning)
		{
			m_DelayedApplySelection.Clear();
			ApplySelection();
		}
		Items.ForEach(delegate(TViewModel vm)
		{
			vm.Dispose();
		});
		Items.Clear();
		m_SelectionStateFeature?.ClearSelection();
		m_SelectedItem.Value = null;
	}

	protected virtual void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager == null)
		{
			return;
		}
		IEnumerable<BlueprintSelectionFeature> featureSelectionsByGroup = UtilityChargen.GetFeatureSelectionsByGroup(manager.Path, FeatureGroup, manager.PreviewUnit);
		if (!featureSelectionsByGroup.Any())
		{
			return;
		}
		m_Selection = featureSelectionsByGroup.First();
		List<FeatureSelectionItem> list = m_Selection.GetSelectionItems(manager.PreviewUnit, manager.Path).ToList();
		m_SelectionStateFeature = manager.GetSelectionState(manager.Path, m_Selection, 0) as SelectionStateFeature;
		list.RemoveAll(delegate(FeatureSelectionItem i)
		{
			if (m_SelectionStateFeature != null)
			{
				CalculatedPrerequisite calculatedPrerequisite = m_SelectionStateFeature.GetCalculatedPrerequisite(i);
				if (calculatedPrerequisite != null)
				{
					return !calculatedPrerequisite.Value;
				}
				return false;
			}
			return true;
		});
		foreach (FeatureSelectionItem item in list)
		{
			Items.Add(CreateItem(item, m_SelectionStateFeature, PhaseType));
		}
		UpdateIsCompleted();
	}

	private void RefreshItems(LevelUpManager manager)
	{
		List<FeatureSelectionItem> selectionItems = m_Selection.GetSelectionItems(manager.PreviewUnit, manager.Path).ToList();
		m_SelectionStateFeature = manager.GetSelectionState(manager.Path, m_Selection, 0) as SelectionStateFeature;
		selectionItems.RemoveAll(delegate(FeatureSelectionItem i)
		{
			CalculatedPrerequisite calculatedPrerequisite = m_SelectionStateFeature.GetCalculatedPrerequisite(i);
			return calculatedPrerequisite != null && !calculatedPrerequisite.Value;
		});
		Items.RemoveAll((TViewModel i) => !selectionItems.HasItem((FeatureSelectionItem newItem) => i.Feature == newItem.Feature));
		foreach (FeatureSelectionItem newItem in selectionItems)
		{
			if (!Items.HasItem((TViewModel i) => i.Feature == newItem.Feature))
			{
				Items.Add(CreateItem(newItem, m_SelectionStateFeature, PhaseType));
			}
		}
		if (!Items.Contains(SelectedItem.CurrentValue))
		{
			SelectionGroup.TrySelectFirstValidEntity();
		}
	}

	protected void RefreshVisualSettings()
	{
		if (CanShowVisualSettings)
		{
			UtilityChargen.GetClothesColorsProfile(CharGenContext.Doll.Clothes, out var colorPreset);
			SetShowVisualSettings(!CharGenContext.Doll.ShowCloth || colorPreset != null);
		}
		else
		{
			SetShowVisualSettings(show: false);
		}
	}

	protected void HandleSelectedItem(TViewModel item)
	{
		if (item != null && FeatureGroup != 0)
		{
			Game.Instance.GameCommandQueue.CharGenSelectItem(FeatureGroup, item.Feature);
		}
		else
		{
			DelayedApplySelection();
		}
	}

	public bool GoNextPage()
	{
		return SelectionGroup.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return SelectionGroup.SelectPrevValidEntity();
	}

	private void DelayedApplySelection()
	{
		m_DelayedApplySelection.Stop();
		m_DelayedApplySelection.InvokeNextFrame(ApplySelection);
		SetupTooltipTemplate();
	}

	private void ApplySelection()
	{
		if (SelectedItem.CurrentValue != null)
		{
			SelectedItem.CurrentValue.ApplySelection();
			LevelUpManager currentValue = CharGenContext.LevelUpManager.CurrentValue;
			if (currentValue != null)
			{
				CharGenContext.Doll.UpdateMechanicsEntities(currentValue.PreviewUnit);
				OnSelectionApplied?.Invoke();
			}
			RefreshVisualSettings();
			m_DelayedApplySelection.Clear();
			UpdateIsCompleted();
		}
	}

	protected void TrySelectItem()
	{
		if (SelectedItem.CurrentValue == null)
		{
			SelectionGroup.TrySelectFirstValidEntity();
		}
	}

	protected abstract TViewModel CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType);

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	protected void SetupTooltipTemplate()
	{
		ReactiveTooltipTemplate.Value = TooltipTemplate();
	}

	protected virtual TooltipBaseTemplate TooltipTemplate()
	{
		if (SelectedItem.CurrentValue == null)
		{
			return null;
		}
		return new TooltipTemplateChargenBackground(SelectedItem.CurrentValue.Feature);
	}
}
