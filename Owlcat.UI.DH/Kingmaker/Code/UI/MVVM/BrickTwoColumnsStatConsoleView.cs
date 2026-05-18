using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTwoColumnsStatConsoleView : BrickTwoColumnsStatView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_LeftMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_RightMultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return null;
	}
}
