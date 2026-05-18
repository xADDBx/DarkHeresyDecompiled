using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconPatternConsoleView : BrickIconPatternView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_FrameButton;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonFirstFocus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonSecondFocus;

	public IConsoleEntity GetConsoleEntity()
	{
		return null;
	}
}
