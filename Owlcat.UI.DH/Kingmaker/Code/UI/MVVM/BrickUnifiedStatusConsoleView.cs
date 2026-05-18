using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickUnifiedStatusConsoleView : BrickUnifiedStatusView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_MultiButton;
	}
}
