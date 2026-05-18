using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFeatureShortDescriptionConsoleView : BrickFeatureShortDescriptionView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_MultiButton;
	}
}
