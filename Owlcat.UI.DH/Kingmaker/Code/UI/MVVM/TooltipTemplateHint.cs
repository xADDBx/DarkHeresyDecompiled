using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateHint : TooltipBaseTemplate
{
	public string Text { get; }

	public TooltipTemplateHint(string text)
	{
		Text = text;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickTextVM(Text);
	}
}
