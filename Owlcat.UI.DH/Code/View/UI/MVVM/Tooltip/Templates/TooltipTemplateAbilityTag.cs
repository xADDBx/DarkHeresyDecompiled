using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Localization;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateAbilityTag : TooltipBaseTemplate
{
	public readonly BlueprintAbilityTag AbilityTag;

	public TooltipTemplateAbilityTag(BlueprintAbilityTag abilityTag)
	{
		AbilityTag = abilityTag;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(AbilityTag.Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		LocalizedString description = AbilityTag.Description;
		if (description != null)
		{
			yield return new TooltipBrickText(description);
		}
	}
}
