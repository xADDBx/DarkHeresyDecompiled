using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LootObjectConsoleView : LootObjectView, IConsoleEntityProxy, IConsoleEntity
{
	public GridConsoleNavigationBehaviour NavigationBehaviour;

	public IConsoleEntity ConsoleEntityProxy => NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		NavigationBehaviour = base.SlotsGroup.GetNavigation();
	}
}
