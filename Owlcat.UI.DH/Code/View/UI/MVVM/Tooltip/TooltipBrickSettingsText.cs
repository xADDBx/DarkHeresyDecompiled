using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip;

public class TooltipBrickSettingsText : ITooltipBrick
{
	private readonly string m_Text;

	public TooltipBrickSettingsText(string text)
	{
		m_Text = text;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSettingsTextVM(m_Text);
	}
}
