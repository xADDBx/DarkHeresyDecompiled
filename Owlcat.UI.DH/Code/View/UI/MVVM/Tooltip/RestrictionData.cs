using System.Collections.Generic;

namespace Code.View.UI.MVVM.Tooltip;

public class RestrictionData
{
	public readonly List<RestrictionItem> RestrictionItems;

	public readonly bool Inverted;

	public readonly bool All;

	public readonly bool CanEquip;

	public RestrictionData(RestrictionItem restrictionItem, bool inverted, bool all, bool canEquip)
	{
		RestrictionItems = new List<RestrictionItem> { restrictionItem };
		Inverted = inverted;
		All = all;
		CanEquip = canEquip;
	}

	public RestrictionData(List<RestrictionItem> restrictionItems, bool inverted, bool all, bool canEquip)
	{
		RestrictionItems = new List<RestrictionItem>(restrictionItems);
		Inverted = inverted;
		All = all;
		CanEquip = canEquip;
	}
}
