using System.Collections.Generic;
using Assets.Code.View.UI.MVVM;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpTitledValueStatGroupVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly List<TooltipElementStatValueVM> StatGroups = new List<TooltipElementStatValueVM>();

	public TooltipBrickLevelUpTitledValueStatGroupVM(string title, List<(string Name, string Value)> statGroups)
	{
		Name = title;
		foreach (var statGroup in statGroups)
		{
			TooltipElementStatValueVM tooltipElementStatValueVM = new TooltipElementStatValueVM(statGroup.Name, statGroup.Value);
			AddDisposable(tooltipElementStatValueVM);
			StatGroups.Add(tooltipElementStatValueVM);
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		StatGroups.Clear();
	}
}
