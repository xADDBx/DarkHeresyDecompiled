using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Interfaces;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateSimple : TooltipBaseTemplate, ISimpleTooltip
{
	[CanBeNull]
	public string Header { get; }

	[CanBeNull]
	public string Description { get; }

	public TooltipTemplateSimple(string header, string description = null)
	{
		Header = header;
		Description = description;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(Header);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (Description != null)
		{
			yield return new BrickTextVM(Description);
		}
	}
}
