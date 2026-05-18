using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTitleWithIconVM : TooltipBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickTitleWithIconVM(BlueprintFeatureBase feature, MechanicEntity caster = null)
	{
		Name = feature.Name;
		Icon = feature.Icon;
		Tooltip = new TooltipTemplateFeature(feature, withVariants: false, caster);
	}

	public BrickTitleWithIconVM(CareerPathVM career)
	{
		Name = career.Name;
		Icon = career.Icon.CurrentValue;
		Tooltip = new TooltipTemplateCareer(career);
	}
}
