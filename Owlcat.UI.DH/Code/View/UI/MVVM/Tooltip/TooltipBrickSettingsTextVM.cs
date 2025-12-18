using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip;

public class TooltipBrickSettingsTextVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public TooltipBrickSettingsTextVM(string text)
	{
		Text = text;
	}
}
