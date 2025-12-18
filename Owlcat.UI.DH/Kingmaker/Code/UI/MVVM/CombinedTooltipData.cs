using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombinedTooltipData : TooltipData
{
	public readonly List<TooltipBaseTemplate> Templates;

	public CombinedTooltipData(List<TooltipBaseTemplate> templates, TooltipConfig config, ReactiveCommand<Unit> closeCommand = null, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
		: base(templates.FirstOrDefault(), config, closeCommand, ownerNavigationBehaviour)
	{
		Templates = templates;
	}
}
