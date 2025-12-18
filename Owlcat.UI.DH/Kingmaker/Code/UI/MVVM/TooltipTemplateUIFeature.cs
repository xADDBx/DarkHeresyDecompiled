using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUIFeature : TooltipBaseTemplate
{
	public readonly UIFeature UIFeature;

	public TooltipTemplateUIFeature(UIFeature uiFeature)
	{
		UIFeature = uiFeature;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string text = ((UIFeature.Icon != null) ? "" : UIUtilityAbilities.GetAbilityAcronym(UIFeature.Name));
		Sprite icon = ((UIFeature.Icon != null) ? UIFeature.Icon : UIUtilityText.GetIconByText(UIFeature.NameForAcronym));
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = UIFeature.Name,
			TextParams = new TextFieldParams
			{
				FontStyles = FontStyles.Bold
			}
		};
		TooltipTemplateFeature tooltip = new TooltipTemplateFeature(UIFeature.Feature);
		string acronym = text;
		TalentIconInfo talentIconsInfo = UIFeature.TalentIconsInfo;
		yield return new TooltipBrickIconPattern(icon, null, titleValues, null, null, tooltip, IconPatternMode.SkillMode, acronym, talentIconsInfo);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		AddSource(list);
		AddSelected(list);
		return list;
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickText(UIUtilityText.UpdateDescriptionWithUIProperties(UIFeature.Description, null)));
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
	}

	private void AddSelected(List<ITooltipBrick> bricks)
	{
	}
}
