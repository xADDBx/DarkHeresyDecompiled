using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InfoBodyVM : InfoBaseVM
{
	protected override TooltipTemplateType TemplateType => TooltipTemplateType.Info;

	public InfoBodyVM(TooltipBaseTemplate template)
		: base(template)
	{
	}
}
