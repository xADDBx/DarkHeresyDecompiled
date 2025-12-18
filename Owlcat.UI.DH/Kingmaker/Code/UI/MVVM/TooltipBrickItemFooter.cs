using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickItemFooter : TooltipBrickDoubleText
{
	private readonly Sprite m_Icon;

	private readonly string m_AdditionalLine;

	public TooltipBrickItemFooter(string leftLine, string rightLine, Sprite icon, string additionalLine = null)
		: base(leftLine, rightLine)
	{
		m_Icon = icon;
		m_AdditionalLine = additionalLine;
	}

	public override TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickItemFooterVM(m_LeftLine, m_RightLine, m_Icon, m_AdditionalLine);
	}
}
