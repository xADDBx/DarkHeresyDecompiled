using System.Linq;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWeaponSetConsoleView : TooltipBrickWeaponSetView, IConsoleTooltipBrick
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
		SimpleConsoleNavigationEntity entity = new SimpleConsoleNavigationEntity(m_HandSlotView.Slot, m_HandSlotView.TooltipTemplates().LastOrDefault());
		m_NavigationBehaviour.AddEntityVertical(entity);
		if (m_WidgetList.Entries != null)
		{
			m_NavigationBehaviour.AddRow(m_WidgetList.Entries.Select((IBindable e) => (e as CharInfoWeaponSetAbilityPCView)?.NavigationEntity).ToList());
		}
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
