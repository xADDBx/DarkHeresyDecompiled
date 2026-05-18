using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LevelUpFeatureUIData
{
	public readonly TextValueElement Title;

	public readonly TextValueElement Subtitle;

	public readonly string Acronym;

	public readonly string Attribute;

	public readonly Sprite Icon;

	public readonly Color IconColor;

	public readonly IconDecor IconDecor;

	public readonly TalentIconInfo TalentIconInfo;

	public readonly TooltipBaseTemplate Tooltip;

	public LevelUpFeatureUIData(TextValueElement title, string acronym = null, TextValueElement subtitle = null, string attribute = null, Sprite icon = null, Color iconColor = default(Color), IconDecor iconDecor = IconDecor.Default, TalentIconInfo talentIconInfo = null, TooltipBaseTemplate tooltip = null)
	{
		Title = title;
		Acronym = acronym;
		Subtitle = subtitle;
		Attribute = attribute;
		Icon = icon;
		IconDecor = iconDecor;
		IconColor = ((iconColor == default(Color)) ? Color.white : iconColor);
		TalentIconInfo = talentIconInfo;
		Tooltip = tooltip;
	}
}
