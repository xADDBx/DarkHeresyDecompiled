using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickMultipleTextConsoleView : BrickMultipleTextView, IConsoleTooltipBrick
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
