using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionSlotPartConsoleView : InteractionSlotPartView
{
	public GridConsoleNavigationBehaviour NavigationBehaviour;

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (NavigationBehaviour == null)
		{
			NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		else
		{
			NavigationBehaviour.Clear();
		}
		NavigationBehaviour.AddEntityGrid(m_SlotView as LootSlotConsoleView);
		return NavigationBehaviour;
	}

	public IConsoleEntity GetCurrentFocus()
	{
		return NavigationBehaviour.DeepestNestedFocus;
	}
}
