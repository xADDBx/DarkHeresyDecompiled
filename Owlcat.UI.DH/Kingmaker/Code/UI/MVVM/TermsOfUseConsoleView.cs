using Kingmaker.UI.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class TermsOfUseConsoleView : TermsOfUseBaseView
{
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private HintView m_AcceptHint;

	protected override void OnBind()
	{
		base.OnBind();
		BuildNavigation();
	}

	private void BuildNavigation()
	{
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
