using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickFeatureDescriptionVM : TooltipBaseBrickVM
{
	public string Name;

	public string Description;

	public Sprite Icon;

	public Sprite SpecialFrame;

	public Color32 IconColor;

	public string Acronym;

	public TooltipBaseTemplate Tooltip;

	public TooltipBrickFeatureDescriptionVM(BlueprintFeatureBase feature, MechanicEntity caster = null)
	{
		Name = feature.Name;
		Description = feature.Description;
		if (feature.Icon != null)
		{
			Icon = feature.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(feature.Name);
			IconColor = UIUtilityText.GetColorByText(feature.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(feature);
		}
		Tooltip = new TooltipTemplateFeature(feature, withVariants: false, caster);
	}
}
