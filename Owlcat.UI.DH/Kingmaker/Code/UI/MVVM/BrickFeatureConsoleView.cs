using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFeatureConsoleView : BrickFeatureView, IConsoleTooltipBrick
{
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_MultiButton;
	}
}
