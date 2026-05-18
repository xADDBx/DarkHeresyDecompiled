using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
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
	private static readonly CharGenAppearancePageComponent[] _ComponentOrder = new CharGenAppearancePageComponent[13]
	{
		CharGenAppearancePageComponent.Gender,
		CharGenAppearancePageComponent.FaceType,
		CharGenAppearancePageComponent.BodyType,
		CharGenAppearancePageComponent.SkinColour,
		CharGenAppearancePageComponent.HairType,
		CharGenAppearancePageComponent.HairColour,
		CharGenAppearancePageComponent.EyebrowType,
		CharGenAppearancePageComponent.EyebrowColour,
		CharGenAppearancePageComponent.BeardType,
		CharGenAppearancePageComponent.BeardColour,
		CharGenAppearancePageComponent.ScarsType,
		CharGenAppearancePageComponent.Tattoo,
		CharGenAppearancePageComponent.Augmentic
	};

	private readonly ReactiveProperty<CharGenAppearancePageType> m_CurrentPageType = new ReactiveProperty<CharGenAppearancePageType>();

	private readonly Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM> m_ComponentsByType = new Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM>();

	private readonly List<BaseCharGenAppearancePageComponentVM> m_Components = new List<BaseCharGenAppearancePageComponentVM>();

	private SelectionStateDoll m_SelectionStateDoll;

	private SelectionStateGender m_SelectionStateGender;

	private CompositeDisposable m_UpdateComponentsSubscription;

	private CompositeDisposable m_ComponentSubscriptions;

	private readonly ReactiveProperty<PortraitVM> m_PortraitVM = new ReactiveProperty<PortraitVM>();

	public readonly ObservableList<VirtualListElementVMBase> VirtualListCollection = new ObservableList<VirtualListElementVMBase>();

	private readonly ReactiveProperty<DollZoomLevel> m_Zoom = new ReactiveProperty<DollZoomLevel>(DollZoomLevel.Max);

	public ReadOnlyReactiveProperty<CharGenAppearancePageType> CurrentPageType => m_CurrentPageType;

	public ReadOnlyReactiveProperty<PortraitVM> PortraitVM => m_PortraitVM;

	public ReadOnlyReactiveProperty<DollZoomLevel> Zoom => m_Zoom;

	public DollState DollState => m_CharGenContext.Doll;

	public CharGenAppearanceComponentAppearancePhaseVM(CharGenContext charGenContext, SelectionStateGender selectionStateGender)
		: base(charGenContext, CharGenPhaseType.Appearance)
	{
		base.DisplayMode = CharGenDisplayMode.DollOnly;
		base.HasSmallPortrait = true;
		m_SelectionStateGender = selectionStateGender;
		m_CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager).AddTo(this);
		CreateComponents();
	}

	public void HandleAppearanceComponentUpdate(CharGenAppearancePageComponent component)
	{
		if (m_ComponentsByType.TryGetValue(component, out var value))
		{
			CharGenAppearanceComponentFactory.UpdateComponent(component, value, m_CharGenContext);
		}
	}

	void ICharGenAppearancePhaseHandler.HandleAppearancePageChange(CharGenAppearancePageType pageType)
	{
		if (!UtilityNet.IsControlMainCharacter())
		{
			m_CurrentPageType.Value = pageType;
		}
	}

	public void SelectPage(CharGenAppearancePageType pageType)
	{
		m_CurrentPageType.Value = pageType;
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
		ClearComponents();
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
		UpdateVisualSettings();
		ApplyDollState(m_CharGenContext.Doll);
		UpdateComponents();
		CaptureDefaults();
	}

	private void CreateComponents()
	{
		ClearComponents();
		m_ComponentSubscriptions = new CompositeDisposable();
		CharGenAppearancePageComponent[] componentOrder = _ComponentOrder;
		foreach (CharGenAppearancePageComponent charGenAppearancePageComponent in componentOrder)
		{
			BaseCharGenAppearancePageComponentVM component = CharGenAppearanceComponentFactory.GetComponent(charGenAppearancePageComponent, m_CharGenContext);
			if (component != null)
			{
				m_Components.Add(component);
				m_ComponentsByType[charGenAppearancePageComponent] = component;
				VirtualListCollection.Add(component);
				component.OnChanged.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnComponentChanged).AddTo(m_ComponentSubscriptions);
			}
		}
		foreach (BaseCharGenAppearancePageComponentVM component2 in m_Components)
		{
			component2.OnBeginView();
		}
	}

	private void OnComponentChanged(CharGenAppearancePageComponent changedComponent)
	{
		if (base.IsInDetailedView.CurrentValue)
		{
			ApplyVisualForComponent(changedComponent);
		}
		List<CharGenAppearancePageComponent> list = new List<CharGenAppearancePageComponent>();
		switch (changedComponent)
		{
		case CharGenAppearancePageComponent.BodyType:
			list.Add(CharGenAppearancePageComponent.SkinColour);
			break;
		case CharGenAppearancePageComponent.HairType:
			list.Add(CharGenAppearancePageComponent.HairColour);
			break;
		case CharGenAppearancePageComponent.EyebrowType:
			list.Add(CharGenAppearancePageComponent.EyebrowColour);
			break;
		case CharGenAppearancePageComponent.BeardType:
			list.Add(CharGenAppearancePageComponent.BeardColour);
			break;
		case CharGenAppearancePageComponent.Tattoo:
			list.Add(CharGenAppearancePageComponent.TattooColor);
			break;
		}
		UpdateComponents(list);
	}

	private void ApplyVisualForComponent(CharGenAppearancePageComponent component)
	{
		DollState doll = m_CharGenContext.Doll;
		switch (component)
		{
		case CharGenAppearancePageComponent.HairType:
		case CharGenAppearancePageComponent.HairColour:
		case CharGenAppearancePageComponent.EyebrowType:
		case CharGenAppearancePageComponent.EyebrowColour:
		case CharGenAppearancePageComponent.PortType1:
		case CharGenAppearancePageComponent.PortType2:
			doll.ShowHelmTemp = false;
			doll.ShowClothTemp = true;
			m_Zoom.Value = DollZoomLevel.Min;
			break;
		case CharGenAppearancePageComponent.ScarsType:
		case CharGenAppearancePageComponent.Tattoo:
		case CharGenAppearancePageComponent.Augmentic:
			doll.ShowHelmTemp = false;
			doll.ShowClothTemp = false;
			m_Zoom.Value = DollZoomLevel.Min;
			break;
		default:
			doll.ShowHelmTemp = true;
			doll.ShowClothTemp = true;
			m_Zoom.Value = DollZoomLevel.Max;
			break;
		}
	}

	protected override void OnEndDetailedView()
	{
		base.OnEndDetailedView();
		DollState doll = m_CharGenContext.Doll;
		doll.ShowHelmTemp = true;
		doll.ShowClothTemp = true;
		m_Zoom.Value = DollZoomLevel.Max;
	}

	private void UpdateComponents()
	{
		UpdateComponents(m_ComponentsByType.Keys);
	}

	private void UpdateComponents(IEnumerable<CharGenAppearancePageComponent> componentTypes)
	{
		foreach (CharGenAppearancePageComponent componentType in componentTypes)
		{
			if (m_ComponentsByType.TryGetValue(componentType, out var value))
			{
				CharGenAppearanceComponentFactory.UpdateComponent(componentType, value, m_CharGenContext);
				value.ContentChanged.Execute(Unit.Default);
			}
		}
	}

	private void ClearComponents()
	{
		m_ComponentsByType.Clear();
		m_Components.Clear();
		m_ComponentSubscriptions?.Dispose();
		m_ComponentSubscriptions = null;
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
			m_UpdateComponentsSubscription.Add(m_CharGenContext.Doll.GetReactiveProperty((DollState dollState) => dollState.Gender).Subscribe(delegate(Gender gender)
			{
				m_SelectionStateGender.SelectGender(gender);
				UpdateComponents();
			}));
			m_UpdateComponentsSubscription.Add(ObservableSubscribeExtensions.Subscribe(m_CharGenContext.Doll.UpdateCommand, delegate
			{
				ApplyDollState(m_CharGenContext.Doll);
			}));
		}
	}

	private void UpdateVisualSettings()
	{
		UtilityChargen.GetClothesColorsProfile(m_CharGenContext.Doll.Clothes, out var colorPreset);
		bool showVisualSettings = !m_CharGenContext.Doll.ShowCloth || colorPreset != null;
		SetShowVisualSettings(showVisualSettings);
	}

	private void ClearPortrait()
	{
		PortraitVM.CurrentValue?.Dispose();
		m_PortraitVM.Value = null;
	}

	public void Randomize()
	{
		foreach (BaseCharGenAppearancePageComponentVM component in m_Components)
		{
			if (component.Type == CharGenAppearancePageComponent.Gender)
			{
				component.Randomize();
			}
		}
		UpdateComponents();
		foreach (BaseCharGenAppearancePageComponentVM component2 in m_Components)
		{
			if (component2.Type != CharGenAppearancePageComponent.Gender)
			{
				component2.Randomize();
			}
		}
	}

	public void ResetToDefault()
	{
		foreach (BaseCharGenAppearancePageComponentVM component in m_Components)
		{
			component.ResetToDefault();
		}
	}

	private void CaptureDefaults()
	{
		foreach (BaseCharGenAppearancePageComponentVM component in m_Components)
		{
			component.CaptureDefaults();
		}
	}
}
