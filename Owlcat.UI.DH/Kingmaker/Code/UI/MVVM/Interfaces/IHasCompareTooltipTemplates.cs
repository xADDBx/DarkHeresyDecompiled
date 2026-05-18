using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.Interfaces;

public interface IHasCompareTooltipTemplates
{
	IReadOnlyList<TooltipBaseTemplate> MainTemplates { get; }

	IReadOnlyList<TooltipBaseTemplate> CompareTemplates { get; }
}
