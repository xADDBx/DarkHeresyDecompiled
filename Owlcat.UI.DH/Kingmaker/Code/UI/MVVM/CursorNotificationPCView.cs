using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CursorNotificationPCView : CursorNotificationBaseView
{
	[SerializeField]
	private Canvas m_Canvas;

	private RectTransform m_CanvasRect;

	protected override Vector2 GetCursorPosition()
	{
		if (!m_CanvasRect)
		{
			m_CanvasRect = m_Canvas.GetComponent<RectTransform>();
		}
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, base.ViewModel.GetCursorRawPosition(), m_Canvas.worldCamera, out var localPoint))
		{
			PFLog.UI.Error("CursorNotificationPCView: failed to calculate cursor position in canvas");
		}
		return localPoint + GetPositionOffset();
	}
}
