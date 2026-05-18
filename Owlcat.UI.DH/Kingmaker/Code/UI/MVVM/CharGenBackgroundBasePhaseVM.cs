using System;
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
	private readonly ReactiveProperty<bool> m_CurrentPageIsFirst = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CurrentPageIsLast = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<CharGenPhaseBaseVM> m_CurrentPhase;

	protected readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	protected readonly ObservableList<TViewModel> m_Items = new ObservableList<TViewModel>();

	private readonly ReactiveProperty<TViewModel> m_SelectedItem = new ReactiveProperty<TViewModel>();

	public readonly SelectionGroupRadioVM<TViewModel> SelectionGroup;

	protected readonly FeatureGroup m_FeatureGroup;

	private readonly IDelayedSelector m_DelayedApplySelection;

	private SelectionStateFeature m_SelectionStateFeature;

	protected TViewModel m_HoveredItem;

	protected bool m_CanShowVisualSettings = true;

	protected Action m_OnSelectionApplied;

	protected bool m_Subscribed;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsFirst => m_CurrentPageIsFirst;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsLast => m_CurrentPageIsLast;

	public ReadOnlyReactiveProperty<TViewModel> SelectedItem => m_SelectedItem;

	protected CharGenBackgroundBasePhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType, InfoSectionVM infoSectionVM, ReactiveProperty<CharGenPhaseBaseVM> currentPhase = null)
		: base(charGenContext, phaseType, allowSwitchOff: true)
	{
		InfoVM = infoSectionVM;
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
		m_SelectionStateFeature = selectionStateFeature;
		m_FeatureGroup = selectionStateFeature.Blueprint.Group;
		base.BlueprintSelectionWithUI = selectionStateFeature.Blueprint;
		SelectionGroup = new SelectionGroupRadioVM<TViewModel>(m_Items, m_SelectedItem).AddTo(this);
		SelectedItem.Subscribe(HandleSelectedItem).AddTo(this);
		SelectedItem.Subscribe(delegate(TViewModel value)
		{
			m_CurrentPageIsFirst.Value = m_Items.FirstOrDefault() == value;
			m_CurrentPageIsLast.Value = m_Items.LastOrDefault() == value;
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	void ICharGenSelectItemHandler.HandleSelectItem(FeatureGroup featureGroup, BlueprintFeature blueprintFeature)
	{
		if (m_FeatureGroup != featureGroup || !base.IsInDetailedView.CurrentValue)
		{
			return;
		}
		if (!UtilityNet.IsControlMainCharacter())
		{
			TViewModel value = ((blueprintFeature != null) ? m_Items.FirstOrDefault((TViewModel item) => blueprintFeature == item?.Feature) : null);
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
		if (m_IsAvailable.CurrentValue)
		{
			SelectionStateFeature selectionStateFeature = m_SelectionStateFeature;
			if (selectionStateFeature != null && selectionStateFeature.IsMade)
			{
				return selectionStateFeature.IsValid;
			}
			return false;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		if (!m_Subscribed)
		{
			CreateTooltipSystem();
			m_Subscribed = true;
		}
		RefreshItems();
		RefreshVisualSettings();
		SetupTooltipTemplate();
		m_Items.ForEach(delegate(TViewModel i)
		{
			UpdateItem(i);
		});
	}

	protected override void OnEndDetailedView()
	{
		base.OnEndDetailedView();
		m_ReactiveTooltipTemplate.Value = null;
	}

	protected virtual void Clear()
	{
		if (m_DelayedApplySelection.IsRunning)
		{
			m_DelayedApplySelection.Clear();
			ApplySelection();
		}
		m_Items.ForEach(delegate(TViewModel vm)
		{
			vm.Dispose();
		});
		m_Items.Clear();
		m_SelectionStateFeature?.ClearSelection();
		m_SelectedItem.Value = null;
	}

	protected virtual void HandleLevelUpManager(LevelUpManager manager)
	{
	}

	private void RefreshItems()
	{
		m_Items.RemoveAll((TViewModel i) => !m_SelectionStateFeature.Items.HasItem((FeatureSelectionItem newItem) => i.Feature == newItem.Feature));
		foreach (FeatureSelectionItem newItem in m_SelectionStateFeature.Items)
		{
			if (!m_Items.HasItem((TViewModel i) => i.Feature == newItem.Feature))
			{
				m_Items.Add(CreateItem(newItem, m_SelectionStateFeature, PhaseType));
			}
		}
	}

	protected void RefreshVisualSettings()
	{
		if (m_CanShowVisualSettings)
		{
			UtilityChargen.GetClothesColorsProfile(m_CharGenContext.Doll.Clothes, out var colorPreset);
			SetShowVisualSettings(!m_CharGenContext.Doll.ShowCloth || colorPreset != null);
		}
		else
		{
			SetShowVisualSettings(show: false);
		}
	}

	protected void HandleSelectedItem(TViewModel item)
	{
		if (item == null)
		{
			m_SelectionStateFeature.ClearSelection();
			UpdateIsCompleted();
		}
		if (m_FeatureGroup != 0 && item != null)
		{
			Game.Instance.GameCommandQueue.CharGenSelectItem(m_FeatureGroup, item.Feature);
			return;
		}
		DelayedApplySelection();
		m_Items.ForEach(delegate(TViewModel i)
		{
			UpdateItem(i);
		});
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
			LevelUpManager currentValue = m_CharGenContext.LevelUpManager.CurrentValue;
			if (currentValue != null)
			{
				m_CharGenContext.Doll.UpdateMechanicsEntities(currentValue.PreviewUnit);
				m_OnSelectionApplied?.Invoke();
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
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(delegate(TooltipBaseTemplate t)
		{
			InfoVM.SetTemplate(t);
		}));
	}

	protected void SetupTooltipTemplate()
	{
		if (base.IsInDetailedView.CurrentValue)
		{
			m_ReactiveTooltipTemplate.Value = TooltipTemplate();
			m_ReactiveTooltipTemplate.ForceNotify();
		}
	}

	protected virtual TooltipBaseTemplate TooltipTemplate()
	{
		return (m_HoveredItem ?? SelectedItem.CurrentValue)?.Template ?? new TooltipTemplateLevelUpPhaseDescription(base.BlueprintSelectionWithUI);
	}

	protected void OnHoverItem(TViewModel item)
	{
		m_HoveredItem = item;
		SetupTooltipTemplate();
		m_Items.ForEach(delegate(TViewModel i)
		{
			UpdateItem(i);
		});
	}

	protected void UpdateItem(CharGenBackgroundBaseItemVM itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (m_SelectionStateFeature.CanSelect(itemVM.FeatureSelectionItem))
		{
			itemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
			return;
		}
		bool flag = m_SelectionStateFeature.GetCalculatedPrerequisite(itemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
		itemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
	}
}
