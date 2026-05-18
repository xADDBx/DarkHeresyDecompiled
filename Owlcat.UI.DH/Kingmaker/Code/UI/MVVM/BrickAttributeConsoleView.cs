using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAttributeConsoleView : BrickAttributeView, IConsoleTooltipBrick
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_MultiButton;
	}
}
