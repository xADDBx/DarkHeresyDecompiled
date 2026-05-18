using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsConsoleView : DlcManagerTabSwitchOnDlcsBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private DlcManagerTabSwitchOnDlcsDlcSelectorConsoleView m_DlcsSelectorConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		m_DlcsSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_DlcsSelectorConsoleView.GetNavigationEntities();
	}
}
