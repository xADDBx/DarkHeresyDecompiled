using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.DollRoom;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAppearancePhaseDetailedView : CharGenPhaseDetailedView<CharGenAppearanceComponentAppearancePhaseVM>, ICharGenAppearancePageComponentHandler, ISubscriber
{
	[SerializeField]
	protected CharGenAppearancePageSelectorView m_PageSelectorView;

	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[Header("Components")]
	[SerializeField]
	private StringSequentialSelectorView m_StringSequentialSelectorView;

	[SerializeField]
	private SlideSelectorCommonView m_SlideSelectorCommonView;

	[SerializeField]
	private TextureSequentialSelectorView m_TextureSequentialSelectorView;

	[SerializeField]
	private TextureSelectorCommonView m_TextureSelectorCommonView;

	[SerializeField]
	private TextureSelectorPagedView m_TextureSelectorPagedView;

	[SerializeField]
	private TextureSelectorTabsView m_TextureSelectorTabsView;

	[SerializeField]
	private PortraitSelectorCommonView m_PortraitSelectorCommonView;

	[SerializeField]
	private CharGenVoiceSelectorCommonView m_CharGenVoiceSelectorCommonView;

	protected override bool HasYScrollBindInternal => false;

	public void HandleComponentChanged(CharGenAppearancePageComponent pageComponent)
	{
		switch (pageComponent)
		{
		case CharGenAppearancePageComponent.FaceType:
		case CharGenAppearancePageComponent.ScarsType:
			base.ViewModel.DollState.ShowHelmTemp = true;
			m_CharacterController.ZoomMin();
			break;
		case CharGenAppearancePageComponent.HairType:
		case CharGenAppearancePageComponent.HairColour:
		case CharGenAppearancePageComponent.EyebrowType:
		case CharGenAppearancePageComponent.EyebrowColour:
		case CharGenAppearancePageComponent.PortType1:
		case CharGenAppearancePageComponent.PortType2:
			base.ViewModel.DollState.ShowHelmTemp = false;
			m_CharacterController.ZoomMin();
			break;
		case CharGenAppearancePageComponent.BodyType:
		case CharGenAppearancePageComponent.Tattoo:
			base.ViewModel.DollState.ShowHelmTemp = true;
			m_CharacterController.ZoomMax();
			break;
		default:
			base.ViewModel.DollState.ShowHelmTemp = true;
			break;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_VirtualList.Initialize(new VirtualListElementTemplate<StringSequentialSelectorVM>(m_StringSequentialSelectorView), new VirtualListElementTemplate<SlideSequentialSelectorVM>(m_SlideSelectorCommonView), new VirtualListElementTemplate<TextureSequentialSelectorVM>(m_TextureSequentialSelectorView), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorCommonView, 0), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorPagedView, 1), new VirtualListElementTemplate<CharGenPortraitsSelectorVM>(m_PortraitSelectorCommonView), new VirtualListElementTemplate<CharGenVoiceSelectorVM>(m_CharGenVoiceSelectorCommonView), new VirtualListElementTemplate<TextureSelectorTabsVM>(m_TextureSelectorTabsView));
		m_PortraitFullView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_PageSelectorView.Bind(base.ViewModel.PagesSelectionGroupRadioVM);
		m_VirtualList.Subscribe(base.ViewModel.VirtualListCollection).AddTo(this);
		base.ViewModel.PortraitVM.Subscribe(m_PortraitFullView.Bind).AddTo(this);
		base.ViewModel.OnPageChanged.Subscribe(HandlePageChanged).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		base.ViewModel.DollState.ShowHelmTemp = true;
		base.ViewModel.DollState.ShowClothTemp = true;
		m_CharacterController.ZoomMax();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
	}

	public override ReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty()
	{
		return new ReactiveProperty<bool>(value: true);
	}

	protected virtual void HandlePageChanged(CharGenAppearancePageType pageType)
	{
		switch (pageType)
		{
		case CharGenAppearancePageType.Hair:
			base.ViewModel.DollState.ShowHelmTemp = false;
			base.ViewModel.DollState.ShowClothTemp = true;
			m_CharacterController.ZoomMin();
			break;
		case CharGenAppearancePageType.Tattoo:
			base.ViewModel.DollState.ShowClothTemp = false;
			m_CharacterController.ZoomMax();
			break;
		case CharGenAppearancePageType.Implants:
			base.ViewModel.DollState.ShowClothTemp = false;
			m_CharacterController.ZoomMin();
			break;
		default:
			base.ViewModel.DollState.ShowHelmTemp = true;
			base.ViewModel.DollState.ShowClothTemp = true;
			m_CharacterController.ZoomMax();
			break;
		}
	}
}
