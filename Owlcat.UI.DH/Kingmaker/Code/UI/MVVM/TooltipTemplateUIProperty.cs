using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Scaling.Components;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.UI;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUIProperty : TooltipBaseTemplate
{
	private readonly LocalizedString m_Description;

	private readonly BlueprintMechanicEntityFact m_DescriptionFact;

	private readonly MechanicEntity m_Owner;

	private readonly BlueprintMechanicEntityFact m_FactBlueprint;

	private readonly AbilityData m_Ability;

	public TooltipTemplateUIProperty(UIPropertySettings propertySettings, MechanicEntity owner, BlueprintMechanicEntityFact factBlueprint, AbilityData ability)
	{
		m_Description = propertySettings.Description;
		m_DescriptionFact = propertySettings.DescriptionFact;
		m_Owner = owner;
		m_FactBlueprint = factBlueprint;
		m_Ability = ability;
	}

	public TooltipTemplateUIProperty(AbilityPropertyUISettings uiSettings, MechanicEntity owner, BlueprintMechanicEntityFact factBlueprint, AbilityData ability)
	{
		m_Description = uiSettings.Description;
		m_DescriptionFact = uiSettings.DescriptionFact;
		m_Owner = owner;
		m_FactBlueprint = factBlueprint;
		m_Ability = ability;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Owner;
			GameLogContext.DescriptionFactBlueprint = m_FactBlueprint;
			GameLogContext.DescriptionAbility = m_Ability;
			using (EvalContext.Build().Ability(m_Ability).Push())
			{
				List<ITooltipBrick> list = new List<ITooltipBrick>();
				AddDescription(list);
				if (type == TooltipTemplateType.Info)
				{
					AddSource(list);
				}
				return list;
			}
		}
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickTextVM(m_Description, TooltipTextType.BoldCentered));
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (m_DescriptionFact != null)
		{
			bricks.Add(new BrickSeparatorVM());
			bricks.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2));
			bricks.Add(new BrickFeatureVM(m_DescriptionFact));
		}
	}
}
