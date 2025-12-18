using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PlayerStashConsoleView : PlayerStashView
{
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	protected override void OnBind()
	{
		base.OnBind();
		m_ChestStashView.SlotsGroup.GetNavigation().DeepestFocusAsObservable.Subscribe(ForceScrollToObj).AddTo(this);
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		return m_ChestStashView.SlotsGroup.GetNavigation();
	}

	public IConsoleEntity GetCurrentFocus()
	{
		return GetNavigation().DeepestNestedFocus;
	}

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
