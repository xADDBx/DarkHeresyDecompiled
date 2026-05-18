using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconPatternVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly IconPatternMode IconMode;

	public readonly string Acronym;

	public readonly TalentIconInfo TalentIconInfo;

	public readonly UIUtilityItem.UIPatternData PatternData;

	public readonly TextEntity Title;

	public readonly TextValueElement SecondaryValuesElement;

	public readonly TextValueElement TertiaryValuesElement;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickIconPatternVM(Sprite icon, UIUtilityItem.UIPatternData patternData, TextEntity title, TextValueElement secondaryValuesElement = null, TextValueElement tertiaryValuesElement = null, TooltipBaseTemplate tooltip = null, IconPatternMode iconMode = IconPatternMode.SkillMode, string acronym = null, TalentIconInfo iconsInfo = null)
	{
		Icon = icon;
		PatternData = patternData;
		Title = title;
		SecondaryValuesElement = secondaryValuesElement;
		TertiaryValuesElement = tertiaryValuesElement;
		Tooltip = tooltip;
		IconMode = iconMode;
		Acronym = acronym;
		TalentIconInfo = iconsInfo;
	}

	public BrickIconPatternVM(Sprite icon, UIUtilityItem.UIPatternData patternData, string title, string secondary = null, string tertiary = null, TooltipBaseTemplate tooltip = null, IconPatternMode iconMode = IconPatternMode.SkillMode)
		: this(icon, patternData, new TextEntity(title), new TextValueElement(secondary), new TextValueElement(tertiary), tooltip, iconMode)
	{
	}
}
