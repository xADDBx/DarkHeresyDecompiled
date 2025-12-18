using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.UI;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUIProperty : TooltipBaseTemplate
{
	public readonly UIPropertySettings PropertySettings;

	private readonly MechanicEntity _owner;

	private readonly BlueprintMechanicEntityFact _factBlueprint;

	private readonly AbilityData _ability;

	public TooltipTemplateUIProperty(UIPropertySettings propertySettings, MechanicEntity owner, BlueprintMechanicEntityFact factBlueprint, AbilityData ability)
	{
		PropertySettings = propertySettings;
		_owner = owner;
		_factBlueprint = factBlueprint;
		_ability = ability;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)_owner;
			GameLogContext.DescriptionFactBlueprint = _factBlueprint;
			GameLogContext.DescriptionAbility = _ability;
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			AddDescription(list);
			if (type == TooltipTemplateType.Info)
			{
				AddSource(list);
			}
			return list;
		}
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickText(PropertySettings.Description, TooltipTextType.BoldCentered));
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (PropertySettings.DescriptionFact != null)
		{
			bricks.Add(new TooltipBrickSeparator());
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2));
			bricks.Add(new TooltipBrickFeature(PropertySettings.DescriptionFact));
		}
	}
}
