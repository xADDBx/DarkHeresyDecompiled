using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickText : ITooltipBrick
{
	private readonly string m_Text;

	private readonly TooltipTextType m_Type;

	private readonly bool m_IsHeader;

	private readonly TooltipTextAlignment m_Alignment;

	private readonly bool m_NeedChangeSize;

	private readonly int m_TextSize;

	private readonly bool m_IsOverline;

	public TooltipBrickText(string text, TooltipTextType type = TooltipTextType.Simple, bool isHeader = false, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, bool needChangeSize = false, int textSize = 18, bool isOverline = false)
	{
		m_Text = text;
		m_Type = type;
		m_IsHeader = isHeader;
		m_Alignment = alignment;
		m_NeedChangeSize = needChangeSize;
		m_TextSize = textSize;
		m_IsOverline = isOverline;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTextVM(m_Text, m_Type, m_Alignment, m_IsHeader, m_NeedChangeSize, m_TextSize, m_IsOverline);
	}
}
