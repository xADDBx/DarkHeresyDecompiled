using System;
using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Vendor;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Modding;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Selection;
using Kingmaker.UI.Workarounds;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CommonPCView : CommonBaseView
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityCommonView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityBugReportView;

	[SerializeField]
	private TextMeshProUGUI m_UIVisibilityText;

	[SerializeField]
	private FadeAnimator m_UIVisibilityFadeAnimator;

	[SerializeField]
	private CounterWindowPCView m_CounterWindowPCView;

	[SerializeField]
	private ContextMenuPCView m_ContextMenuPCView;

	[SerializeField]
	private FadeView m_FadeView;

	[SerializeField]
	private DragNDropManager m_DragNDropManager;

	[SerializeField]
	private MultiplySelection m_MultiplySelection;

	[SerializeField]
	private OwlcatModificationsWindow m_OwlcatModificationsWindow;

	private bool m_IsInit;

	[SerializeField]
	private UIViewLinkTemp<NetLobbyPCView, NetLobbyVM> m_NetLobbyPCView;

	[SerializeField]
	private UIViewLinkTemp<NetRolesPCView, NetRolesVM> m_NetRolesPCView;

	[SerializeField]
	private UIViewLinkTemp<DlcManagerPCView, DlcManagerVM> m_DlcManagerPCView;

	private Coroutine m_DisappearAnimationCoroutine;

	private IDisposable m_EscHotkey;

	[Space]
	[SerializeField]
	private UIViewLinkTemp<CreditsPCView, CreditsVM> m_CreditsPCView;

	[SerializeField]
	private UIViewLinkTemp<VendorBaseScreenView, VendorBaseScreenVM> m_VendorPCViewLink;

	[SerializeField]
	private UIViewLinkTemp<VendorSelectingWindowPCView, VendorSelectingWindowVM> m_VendorSelectingWindowContextPCView;

	[SerializeField]
	private GameOverPCView m_GameOverPCView;

	[SerializeField]
	private RectTransform m_StaticCanvasRT;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityDynamicView;

	[Header("Canvas scaler")]
	[SerializeField]
	private CanvasScalerWorkaround m_DynamicCanvasScalerWorkaround;

	[Space]
	[SerializeField]
	private LineOfSightControllerView m_SightControllerPCView;

	[SerializeField]
	private ChannelingLinesControllerView m_ChannelingLinesControllerView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_UIVisibilityCommonView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityBugReportView.Bind(base.ViewModel.UIVisibilityVM);
		m_FadeView.Bind(base.ViewModel.FadeVM);
		AddDisposable(base.ViewModel.CounterWindowVM.Subscribe(m_CounterWindowPCView.Bind));
		AddDisposable(base.ViewModel.ContextMenuVM.Subscribe(m_ContextMenuPCView.Bind));
		AddDisposable(m_OwlcatModificationsWindow.Bind());
		AddDisposable(base.ViewModel.NetLobbyVM.Subscribe(m_NetLobbyPCView.Bind));
		AddDisposable(base.ViewModel.NetRolesVM.Subscribe(m_NetRolesPCView.Bind));
		AddDisposable(base.ViewModel.DlcManagerVM.Subscribe(m_DlcManagerPCView.Bind));
		AddDisposable(UIVisibilityState.VisibilityPreset.Skip(1).Subscribe(delegate
		{
			UIVisibilityChange();
		}));
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsPCView.Bind));
		AddDisposable(base.ViewModel.VendorVM.Subscribe(m_VendorPCViewLink.Bind));
		AddDisposable(base.ViewModel.VendorSelectingWindowVM.Subscribe(m_VendorSelectingWindowContextPCView.Bind));
		m_UIVisibilityDynamicView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_DragNDropManager.Dispose();
		if (m_DisappearAnimationCoroutine != null)
		{
			StopCoroutine(m_DisappearAnimationCoroutine);
			m_DisappearAnimationCoroutine = null;
		}
		m_EscHotkey?.Dispose();
		m_EscHotkey = null;
	}

	private void UIVisibilityChange()
	{
		if (UIVisibilityState.VisibilityPresetIndex != 9)
		{
			if (m_EscHotkey == null)
			{
				m_EscHotkey = EscHotkeyManager.Instance.Subscribe(ResetUIVisibility);
			}
		}
		else
		{
			m_EscHotkey?.Dispose();
			m_EscHotkey = null;
		}
		m_UIVisibilityFadeAnimator.AppearAnimation();
		if (m_DisappearAnimationCoroutine != null)
		{
			StopCoroutine(m_DisappearAnimationCoroutine);
		}
		m_DisappearAnimationCoroutine = StartCoroutine(DisappearAnimationCoroutine());
		string text = UIStrings.Instance.CommonTexts.UIVisibility.Text;
		m_UIVisibilityText.text = text + "<br>" + UIVisibilityState.VisibilityPresetIndex + "/" + 9;
	}

	private void ResetUIVisibility()
	{
		UIVisibilityState.ShowAllUI();
	}

	private IEnumerator DisappearAnimationCoroutine()
	{
		yield return new WaitForSecondsRealtime(1f);
		m_UIVisibilityFadeAnimator.DisappearAnimation();
		m_DisappearAnimationCoroutine = null;
	}
}
