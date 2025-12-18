using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryStashConsoleView : InventoryStashView
{
	public ItemsFilterConsoleView ItemsFilter => m_ItemsFilter as ItemsFilterConsoleView;

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		if (base.ViewModel == null)
		{
			return null;
		}
		if (m_ItemSlotsGroup.ViewModel == null)
		{
			return m_InsertableSlotsGroup.VirtualList.GetNavigationBehaviour();
		}
		return m_ItemSlotsGroup.VirtualList.GetNavigationBehaviour();
	}

	public IConsoleEntity GetCurrentFocus()
	{
		return GetNavigation().DeepestNestedFocus;
	}
}
