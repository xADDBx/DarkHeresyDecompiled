using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickHint : ITooltipBrick
{
	private readonly string m_Text;

	public TooltipBrickHint(string text)
	{
		m_Text = text;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickHintVM(m_Text);
	}
}
