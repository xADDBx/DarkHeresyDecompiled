using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InfoWindowPCView : InfoWindowBaseView
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private RectTransform m_Window;

	protected override void OnBind()
	{
		base.OnBind();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			Close();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.Close).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Hide();
	}

	protected override void SetPosition()
	{
		if (!m_IsStartPosition && !(m_Window == null) && base.ViewModel.LastTooltipPosition.HasValue && !(base.ViewModel.LastTooltipPosition.Value == Vector2.zero))
		{
			Vector3 vector = base.transform.parent.InverseTransformPoint(base.ViewModel.LastTooltipPosition.Value);
			vector.y = 0f;
			m_Window.anchoredPosition = UIUtilityRect.LimitPositionRectInRect(vector, (RectTransform)base.transform, m_Window);
		}
	}

	protected override void OnClose()
	{
		base.OnClose();
		base.ViewModel.OnClose();
	}
}
