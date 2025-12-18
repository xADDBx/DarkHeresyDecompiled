using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickDoubleText : ITooltipBrick
{
	protected readonly string m_LeftLine;

	protected readonly string m_RightLine;

	private readonly TextAnchor m_LeftAlignment;

	private readonly TextAnchor m_RightAlignment;

	public TooltipBrickDoubleText(string leftLine, string rightLine, TextAnchor leftAlignment = TextAnchor.MiddleCenter, TextAnchor rightAlignment = TextAnchor.MiddleCenter)
	{
		m_LeftLine = leftLine;
		m_RightLine = rightLine;
		m_LeftAlignment = leftAlignment;
		m_RightAlignment = rightAlignment;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickDoubleTextVM(m_LeftLine, m_RightLine, m_LeftAlignment, m_RightAlignment);
	}
}
