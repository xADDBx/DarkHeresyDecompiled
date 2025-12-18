using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickUnifiedStatus : ITooltipBrick
{
	private readonly UnifiedStatus m_Status;

	private readonly string m_Text;

	public TooltipBrickUnifiedStatus(UnifiedStatus status, string text)
	{
		m_Status = status;
		m_Text = text;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickUnifiedStatusVM(m_Status, m_Text);
	}
}
