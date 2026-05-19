using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemRestrictionVM : TooltipBrickVM
{
	public readonly List<RestrictionData> PrerequisiteList;

	public readonly bool CanEquipItem;

	public readonly string OwnerName;

	public bool HasOwnerName => !string.IsNullOrEmpty(OwnerName);

	public bool HasFalseRestriction => PrerequisiteList.Any((RestrictionData p) => !p.CanEquip);

	public BrickItemRestrictionVM(List<RestrictionData> prerequisiteList, bool canEquip, string ownerName = null)
	{
		PrerequisiteList = prerequisiteList;
		CanEquipItem = canEquip;
		OwnerName = ownerName;
	}

	public List<string> GetFalseRestrictionStrings()
	{
		if (PrerequisiteList.All((RestrictionData p) => p.CanEquip))
		{
			return new List<string>();
		}
		List<string> list = new List<string>();
		foreach (RestrictionData prerequisite in PrerequisiteList)
		{
			if (!prerequisite.CanEquip)
			{
				List<RestrictionItem> list2 = prerequisite.RestrictionItems.Where((RestrictionItem i) => !i.MeetPrerequisite).ToList();
				switch (list2.Count)
				{
				case 1:
				{
					string text2 = (prerequisite.Inverted ? UIStrings.Instance.Tooltips.RequireInverted : UIStrings.Instance.Tooltips.Require);
					string factString = GetFactString(list2[0]);
					list.Add(text2 + " " + factString);
					break;
				}
				default:
				{
					GetPrefixAndSeparator(prerequisite, out var prefix, out var separator);
					separator = " " + separator + " ";
					List<string> values = list2.Select(GetFactString).ToList();
					string text = string.Join(separator, values);
					list.Add(prefix + " " + text);
					break;
				}
				case 0:
					break;
				}
			}
		}
		return list;
	}

	private void GetPrefixAndSeparator(RestrictionData data, out string prefix, out string separator)
	{
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		prefix = (data.Inverted ? tooltips.RequireInverted : tooltips.Require);
		separator = (data.All ? tooltips.and : tooltips.or);
	}

	private static string GetFactString(RestrictionItem item)
	{
		return UIUtilityText.WrapWithWeight((item.UnitFact == null) ? (item.Value + " " + item.Key) : item.UnitFact.LocalizedName.Text, TextFontWeight.SemiBold);
	}
}
