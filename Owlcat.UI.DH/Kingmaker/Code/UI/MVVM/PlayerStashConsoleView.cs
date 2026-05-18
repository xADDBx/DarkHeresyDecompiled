using Kingmaker.UI.Common;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PlayerStashConsoleView : PlayerStashView
{
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	private void ForceScrollToObj(IConsoleEntity entity)
	{
		if (entity != null)
		{
			LootSlotConsoleView lootSlotConsoleView = entity as LootSlotConsoleView;
			if ((bool)lootSlotConsoleView)
			{
				RectTransform targetRect = lootSlotConsoleView.transform as RectTransform;
				m_ScrollRect.EnsureVisibleVertical(targetRect);
			}
		}
	}
}
