using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpFeatureData
{
	public readonly string Title;

	public readonly string Subtitle;

	public readonly string Value;

	public readonly string Value2;

	public readonly string Acronym;

	public readonly string Attribute;

	public readonly string CenteredHeader;

	public readonly Sprite Icon;

	public readonly bool IconWithFrame;

	public readonly Vector2 IconSize;

	public readonly TalentIconInfo TalentIconInfo;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickLevelUpFeatureData(string title, string value = null, string acronym = null, string subtitle = null, string value2 = null, string attribute = null, Sprite icon = null, bool iconWithFrame = true, Vector2 iconSize = default(Vector2), TalentIconInfo talentIconInfo = null, TooltipBaseTemplate tooltip = null, string centeredHeader = null)
	{
		Title = title;
		Value = value;
		Acronym = acronym;
		Subtitle = subtitle;
		Value2 = value2;
		Attribute = attribute;
		Icon = icon;
		IconWithFrame = iconWithFrame;
		IconSize = iconSize;
		TalentIconInfo = talentIconInfo;
		Tooltip = tooltip;
		CenteredHeader = centeredHeader;
	}
}
