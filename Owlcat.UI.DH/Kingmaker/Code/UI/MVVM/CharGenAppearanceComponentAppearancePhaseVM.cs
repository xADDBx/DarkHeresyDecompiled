using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenAppearanceComponentAppearancePhaseVM : CharGenPhaseBaseVM, ILevelUpDollHandler, ISubscriber, ICharGenPortraitSelectorHoverHandler, ICharGenAppearancePhaseHandler, ICharGenAppearanceComponentUpdateHandler
{
	private readonly ReactiveProperty<bool> m_CurrentPageIsFirst = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CurrentPageIsLast = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<CharGenAppearancePageVM> m_CurrentPageVM = new ReactiveProperty<CharGenAppearancePageVM>();

	private readonly ObservableList<CharGenAppearancePageVM> m_Pages = new ObservableList<CharGenAppearancePageVM>();

	private SelectionStateDoll m_SelectionStateDoll;

	private SelectionStateGender m_SelectionStateGender;

	private bool m_Subscribed;

	private CompositeDisposable m_UpdateComponentsSubscription;

	private readonly ReactiveCommand<CharGenAppearancePageType> m_OnPageChanged = new ReactiveCommand<CharGenAppearancePageType>();

	public readonly SelectionGroupRadioVM<CharGenAppearancePageVM> PagesSelectionGroupRadioVM;

	private readonly ReactiveProperty<PortraitVM> m_PortraitVM = new ReactiveProperty<PortraitVM>();

	public readonly ObservableList<VirtualListElementVMBase> VirtualListCollection = new ObservableList<VirtualListElementVMBase>();

	public ReadOnlyReactiveProperty<bool> CurrentPageIsFirst => m_CurrentPageIsFirst;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsLast => m_CurrentPageIsLast;

	public ReadOnlyReactiveProperty<CharGenAppearancePageVM> CurrentPageVM => m_CurrentPageVM;

	public Observable<CharGenAppearancePageType> OnPageChanged => m_OnPageChanged;

	public ReadOnlyReactiveProperty<PortraitVM> PortraitVM => m_PortraitVM;

	public DollState DollState => CharGenContext.Doll;

	public CharGenAppearanceComponentAppearancePhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Appearance)
	{
		PagesSelectionGroupRadioVM = AddDisposableAndReturn(new SelectionGroupRadioVM<CharGenAppearancePageVM>(m_Pages, m_CurrentPageVM));
		AddDisposable(CurrentPageVM.Subscribe(delegate(CharGenAppearancePageVM value)
		{
			m_CurrentPageIsFirst.Value = m_Pages.FirstOrDefault() == value;
			m_CurrentPageIsLast.Value = m_Pages.LastOrDefault() == value;
		}));
		AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
	}

	public void HandleAppearanceComponentUpdate(CharGenAppearancePageComponent component)
	{
		CurrentPageVM.CurrentValue.UpdateComponent(component);
	}

	void ICharGenAppearancePhaseHandler.HandleAppearancePageChange(CharGenAppearancePageType pageType)
	{
		ClearPortrait();
		VirtualListCollection.Clear();
		CharGenAppearancePageVM charGenAppearancePageVM = m_Pages.FirstOrDefault((CharGenAppearancePageVM p) => p.PageType == pageType);
		if (charGenAppearancePageVM == null)
		{
			PFLog.UI.Error($"CharGenAppearancePageVM not found {pageType}");
			return;
		}
		if (!UtilityNet.IsControlMainCharacter())
		{
			m_CurrentPageVM.Value = charGenAppearancePageVM;
		}
		charGenAppearancePageVM.BeginPageView();
		foreach (BaseCharGenAppearancePageComponentVM component in charGenAppearancePageVM.Components)
		{
			VirtualListCollection.Add(component);
		}
		m_OnPageChanged.Execute(charGenAppearancePageVM.PageType);
		UpdateVisualSettings();
	}

	public void HandleHoverStart(PortraitData portrait)
	{
		m_PortraitVM.Value = new PortraitVM(portrait);
	}

	public void HandleHoverStop()
	{
		ClearPortrait();
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		ApplyDollState(dollState);
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_Pages.Clear();
		VirtualListCollection.Clear();
		ClearPortrait();
	}

	protected override bool CheckIsCompleted()
	{
		if (base.IsInDetailedView.CurrentValue)
		{
			SelectionStateDoll selectionStateDoll = m_SelectionStateDoll;
			if (selectionStateDoll != null && selectionStateDoll.IsMade)
			{
				return selectionStateDoll.IsValid;
			}
			return false;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		CurrentPageVM.CurrentValue?.BeginPageView();
		UpdateVisualSettings();
		ApplyDollState(CharGenContext.Doll);
		if (!m_Subscribed)
		{
			CreatePages();
			PagesSelectionGroupRadioVM.TrySelectFirstValidEntity();
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(CurrentPageVM.Subscribe(OnCurrentPageChanged));
			m_Subscribed = true;
		}
	}

	private void CreatePages()
	{
		foreach (CharGenAppearancePageType item in CharGenAppearancePages.PagesOrder.Where(IsPageEnabled))
		{
			CharGenAppearancePageVM disposable = new CharGenAppearancePageVM(CharGenContext, item, base.IsInDetailedView);
			m_Pages.Add(AddDisposableAndReturn(disposable));
		}
		foreach (CharGenAppearancePageVM page in m_Pages)
		{
			if (page.PageType != CharGenAppearancePageType.General)
			{
				page.CreateComponentsIfNeeded();
			}
		}
	}

	private bool IsPageEnabled(CharGenAppearancePageType pageType)
	{
		if (pageType != CharGenAppearancePageType.NavigatorMutations)
		{
			return true;
		}
		CharGenConfig charGenConfig = CharGenContext.CharGenConfig;
		if (charGenConfig.Mode == CharGenMode.NewCompanion)
		{
			return charGenConfig.CompanionType == CharGenCompanionType.Navigator;
		}
		return false;
	}

	private void OnCurrentPageChanged(CharGenAppearancePageVM pageVM)
	{
		if (pageVM != null)
		{
			Game.Instance.GameCommandQueue.CharGenChangeAppearancePage(pageVM.PageType);
		}
	}

	private void ApplyDollState(DollState dollState)
	{
		if (dollState != null)
		{
			m_SelectionStateDoll?.Select(dollState);
		}
		UpdateIsCompleted();
	}

	private void Clear()
	{
		m_UpdateComponentsSubscription?.Dispose();
		m_UpdateComponentsSubscription = null;
		ResetDetailedViewState();
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager == null)
		{
			return;
		}
		BlueprintSelectionDoll selectionByType = UtilityChargen.GetSelectionByType<BlueprintSelectionDoll>(manager.Path);
		BlueprintGenderSelection selectionByType2 = UtilityChargen.GetSelectionByType<BlueprintGenderSelection>(manager.Path);
		if (selectionByType != null && selectionByType2 != null)
		{
			m_SelectionStateDoll = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateDoll;
			m_SelectionStateGender = manager.GetSelectionState(manager.Path, selectionByType2, 0) as SelectionStateGender;
			m_UpdateComponentsSubscription = new CompositeDisposable();
			m_UpdateComponentsSubscription.Add(CharGenContext.Doll.GetReactiveProperty((DollState dollState) => dollState.Gender).Subscribe(delegate(Gender gender)
			{
				m_SelectionStateGender.SelectGender(gender);
				UpdateComponents();
			}));
			PagesSelectionGroupRadioVM.TrySelectFirstValidEntity();
		}
	}

	private void UpdateComponents()
	{
		foreach (CharGenAppearancePageVM page in m_Pages)
		{
			page.UpdateComponents();
		}
	}

	private void UpdateVisualSettings()
	{
		if (CurrentPageVM.CurrentValue != null)
		{
			UtilityChargen.GetClothesColorsProfile(CharGenContext.Doll.Clothes, out var colorPreset);
			bool showVisualSettings = CurrentPageVM.CurrentValue.PageType != 0 && (!CharGenContext.Doll.ShowCloth || colorPreset != null);
			SetShowVisualSettings(showVisualSettings);
		}
	}

	private void ClearPortrait()
	{
		PortraitVM.CurrentValue?.Dispose();
		m_PortraitVM.Value = null;
	}

	public bool GoNextPage()
	{
		return PagesSelectionGroupRadioVM.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return PagesSelectionGroupRadioVM.SelectPrevValidEntity();
	}
}
