using System.Collections.Generic;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpTitledValueStatGroupVM : TooltipBrickVM
{
	public readonly string Name;

	public readonly List<TooltipElementStatValueVM> StatGroups = new List<TooltipElementStatValueVM>();

	public BrickLevelUpTitledValueStatGroupVM(string title, List<(string Name, string Value)> statGroups)
	{
		Name = title;
		foreach (var statGroup in statGroups)
		{
			TooltipElementStatValueVM item = new TooltipElementStatValueVM(statGroup.Name, statGroup.Value).AddTo(this);
			StatGroups.Add(item);
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		StatGroups.Clear();
	}
}
