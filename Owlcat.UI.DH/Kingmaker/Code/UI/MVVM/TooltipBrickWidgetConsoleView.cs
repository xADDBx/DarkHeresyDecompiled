using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWidgetConsoleView : TooltipBrickWidgetView, IConsoleTooltipBrick
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn(m_WidgetList.GetNavigationEntities());
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
