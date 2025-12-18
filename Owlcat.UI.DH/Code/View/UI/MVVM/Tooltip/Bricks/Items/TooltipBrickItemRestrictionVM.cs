using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using UnityEngine.TextCore.Text;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickItemRestrictionVM : TooltipBaseBrickVM
{
	public readonly List<RestrictionData> PrerequisiteList;

	public readonly bool CanEquipItem;

	public bool HasFalseRestriction => PrerequisiteList.Any((RestrictionData p) => !p.CanEquip);

	public TooltipBrickItemRestrictionVM(List<RestrictionData> prerequisiteList, bool canEquip)
	{
		PrerequisiteList = prerequisiteList;
		CanEquipItem = canEquip;
	}

	public List<string> GetFalseRestrictionStrings()
	{
		if (PrerequisiteList.All((RestrictionData p) => p.CanEquip))
		{
			return new List<string>();
		}
		List<string> list = new List<string>();
		foreach (RestrictionData data in PrerequisiteList)
		{
			if (data.CanEquip)
			{
				continue;
			}
			List<RestrictionItem> list2 = data.RestrictionItems.Where((RestrictionItem i) => !i.MeetPrerequisite || data.All).ToList();
			if (list2.Count == 1)
			{
				string text = (data.Inverted ? UIStrings.Instance.Tooltips.RequireInverted : UIStrings.Instance.Tooltips.Require);
				string text2 = UIUtilityText.WrapWithWeight(list2[0].UnitFact?.LocalizedName.Text, TextFontWeight.SemiBold) ?? "";
				list.Add(text + " " + text2);
				continue;
			}
			GetPrefixAndSeparator(data, out var prefix, out var separator);
			separator = " " + separator + " ";
			List<string> values = list2.Select((RestrictionItem i) => UIUtilityText.WrapWithWeight(i.UnitFact?.LocalizedName.Text, TextFontWeight.SemiBold) ?? "").ToList();
			string text3 = string.Join(separator, values);
			list.Add(prefix + " " + text3);
		}
		return list;
	}

	private void GetPrefixAndSeparator(RestrictionData data, out string prefix, out string separator)
	{
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		prefix = (data.Inverted ? tooltips.RequireInverted : tooltips.Require);
		separator = (data.All ? tooltips.and : tooltips.or);
	}
}
