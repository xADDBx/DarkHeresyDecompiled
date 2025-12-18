using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconPatternVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly IconPatternMode IconMode;

	public readonly string Acronym;

	public readonly TalentIconInfo TalentIconInfo;

	public readonly UIUtilityItem.UIPatternData PatternData;

	public readonly TooltipBrickIconPattern.TextFieldValues TitleValues;

	public readonly TooltipBrickIconPattern.TextFieldValues SecondaryValues;

	public readonly TooltipBrickIconPattern.TextFieldValues TertiaryValues;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickIconPatternVM(Sprite icon, UIUtilityItem.UIPatternData patternData, TooltipBrickIconPattern.TextFieldValues titleValues, TooltipBrickIconPattern.TextFieldValues secondaryValues, TooltipBrickIconPattern.TextFieldValues tertiaryValues, TooltipBaseTemplate tooltip, IconPatternMode iconMode, string acronym, TalentIconInfo talentIconsInfo)
	{
		Icon = icon;
		PatternData = patternData;
		TitleValues = titleValues;
		SecondaryValues = secondaryValues;
		TertiaryValues = tertiaryValues;
		Tooltip = tooltip;
		IconMode = iconMode;
		Acronym = acronym;
		TalentIconInfo = talentIconsInfo;
	}
}
