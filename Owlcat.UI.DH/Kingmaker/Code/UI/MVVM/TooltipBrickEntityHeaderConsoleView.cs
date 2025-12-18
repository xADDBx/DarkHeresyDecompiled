using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickEntityHeaderConsoleView : TooltipBrickEntityHeaderView, IConsoleTooltipBrick
{
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	public bool IsBinded => base.ViewModel != null;

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_MultiButton);
	}
}
