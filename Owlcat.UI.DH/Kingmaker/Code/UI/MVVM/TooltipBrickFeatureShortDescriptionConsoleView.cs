using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickFeatureShortDescriptionConsoleView : TooltipBrickFeatureShortDescriptionView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public bool IsBinded => base.ViewModel != null;

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_MultiButton, base.ViewModel.Tooltip);
	}
}
