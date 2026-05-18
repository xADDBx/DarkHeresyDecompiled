using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyDlcListConsoleView : NetLobbyDlcListBaseView
{
	[SerializeField]
	private NetLobbyDlcListDlcEntityConsoleView m_DlcEntityConsoleViewPrefab;

	protected override void DrawDlcsImpl()
	{
		base.DrawDlcsImpl();
		NetLobbyDlcListDlcEntityVM[] array = base.ViewModel.Dlcs.ToArray();
		if (array.Any())
		{
			m_DlcsWidgetList.DrawEntries(array, m_DlcEntityConsoleViewPrefab);
		}
	}

	protected virtual void CreateInputImpl()
	{
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
