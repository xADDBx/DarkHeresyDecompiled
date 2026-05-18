using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class InfoWindowConsoleView : InfoWindowBaseView
{
	public static readonly string InputLayerContextName = "InfoWindowConsoleView";

	private readonly ReactiveProperty<bool> m_IsShowTooltip = new ReactiveProperty<bool>();

	private ReadOnlyReactiveProperty<bool> m_HasTooltip;

	private readonly ReactiveProperty<bool> m_IsWindowOpen = new ReactiveProperty<bool>();

	private RectTransform m_CurrentFocusedRect;

	private Vector3 m_LastPosition;

	public ReadOnlyReactiveProperty<bool> IsWindowOpen => m_IsWindowOpen;

	protected override void OnBind()
	{
		base.OnBind();
		Game.Instance.RequestPauseUi(isPaused: true);
		m_IsShowTooltip.Value = false;
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Game.Instance.RequestPauseUi(isPaused: false);
		m_LastPosition = base.transform.localPosition;
		TooltipHelper.HideTooltip();
	}

	protected override void OnClose()
	{
		base.OnClose();
		if (RootUIContext.Instance.TooltipIsShown)
		{
			m_IsShowTooltip.Value = false;
			return;
		}
		m_IsWindowOpen.Value = false;
		base.ViewModel.OnClose();
	}

	protected override void SetPosition()
	{
		if (m_IsStartPosition)
		{
			base.transform.localPosition = m_Position;
		}
		else
		{
			base.transform.localPosition = m_LastPosition;
		}
	}
}
