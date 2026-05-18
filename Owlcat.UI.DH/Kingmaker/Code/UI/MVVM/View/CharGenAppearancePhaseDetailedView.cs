using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.DollRoom;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAppearancePhaseDetailedView : CharGenPhaseDetailedView<CharGenAppearanceComponentAppearancePhaseVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[Header("Anchor Bar")]
	[SerializeField]
	protected CharGenAppearanceAnchorBarView m_AnchorBar;

	[Header("Control Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_ButtonRandom;

	[SerializeField]
	private TextMeshProUGUI m_ButtonRandomText;

	[SerializeField]
	private OwlcatMultiButton m_ButtonReset;

	[SerializeField]
	private TextMeshProUGUI m_ButtonUndoText;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[Header("Selectors Virtual List")]
	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[Header("Components")]
	[SerializeField]
	private SlideSelectorCommonView m_SlideSelectorCommonView;

	[SerializeField]
	private TextureSequentialSelectorView m_TextureSequentialSelectorView;

	[SerializeField]
	private TextureSelectorCommonView m_TextureSelectorCommonView;

	[SerializeField]
	private TextureSelectorCommonView m_TextureSelectorColorView;

	[SerializeField]
	private TextureSelectorPagedView m_TextureSelectorPagedView;

	[SerializeField]
	private TextureAndColorGroupSelectorView m_TextureAndColorGroupSelectorView;

	private bool m_IsInitialized;

	protected override bool HasYScrollBindInternal => false;

	public override void Initialize()
	{
		if (!m_IsInitialized)
		{
			base.Initialize();
			m_VirtualList.Initialize(new VirtualListElementTemplate<SlideSequentialSelectorVM>(m_SlideSelectorCommonView), new VirtualListElementTemplate<TextureSequentialSelectorVM>(m_TextureSequentialSelectorView), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorCommonView, 0), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorPagedView, 1), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorColorView, 2), new VirtualListElementTemplate<TextureAndColorGroupSelectorVM>(m_TextureAndColorGroupSelectorView));
			m_IsInitialized = true;
		}
	}

	protected override void OnBind()
	{
		if (!m_IsInitialized)
		{
			Initialize();
		}
		base.OnBind();
		m_HeaderLabel.text = base.ViewModel.PhaseName.CurrentValue;
		m_VirtualList.Subscribe(base.ViewModel.VirtualListCollection).AddTo(this);
		m_AnchorBar.Bind(base.ViewModel.VirtualListCollection);
		m_AnchorBar.ActiveAnchorChanged.Subscribe(delegate(CharGenAppearancePageType pt)
		{
			base.ViewModel.SelectPage(pt);
		}).AddTo(this);
		base.ViewModel.CurrentPageType.Subscribe(delegate(CharGenAppearancePageType pt)
		{
			if (!m_AnchorBar.IsActivePage(pt))
			{
				m_AnchorBar.SetActivePage(pt);
			}
		}).AddTo(this);
		base.ViewModel.Zoom.Subscribe(delegate(DollZoomLevel z)
		{
			if (z == DollZoomLevel.Min)
			{
				m_CharacterController.ZoomMin();
			}
			else
			{
				m_CharacterController.ZoomMax();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ButtonRandom?.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Randomize();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ButtonReset?.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ResetToDefault();
		}).AddTo(this);
		m_ButtonRandomText.text = UIStrings.Instance.CharGen.Randomize;
		m_ButtonUndoText.text = UIStrings.Instance.CharGen.Undo;
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_AnchorBar.Unbind();
	}

	public override ReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty()
	{
		return new ReactiveProperty<bool>(value: true);
	}
}
