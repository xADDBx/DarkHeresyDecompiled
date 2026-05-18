using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickNestedMessageConsoleView : BrickNestedMessageView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_FocusMultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_FocusMultiButton;
	}
}
