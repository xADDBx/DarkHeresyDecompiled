using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Items;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateToggleAbility : TooltipBaseTemplate
{
	private ToggleAbility m_Ability;

	public readonly BlueprintToggleAbility BlueprintAbility;

	private readonly string m_Name;

	private readonly Sprite m_Icon;

	private readonly string m_Description;

	public readonly MechanicEntity Caster;

	public TooltipTemplateToggleAbility(ToggleAbility ability, MechanicEntity caster = null)
	{
		if (ability != null)
		{
			m_Name = ability.Name;
			m_Icon = ability.Icon;
			m_Description = ability.Description;
			Caster = caster;
			BlueprintAbility = ability.Blueprint;
			m_Ability = ability;
		}
	}

	public TooltipTemplateToggleAbility(BlueprintToggleAbility ability, MechanicEntity caster = null)
	{
		if (ability != null)
		{
			m_Name = ability.Name;
			m_Icon = ability.Icon;
			m_Description = ability.Description;
			Caster = caster;
			BlueprintAbility = ability;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickEntityHeader(m_Name, m_Icon, hasUpgrade: false);
		yield return new TooltipBrickSeparator();
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		_ = string.Empty;
		string description = UIUtilityText.UpdateDescriptionWithUIProperties(m_Description, Caster);
		description = UIUtilityText.UpdateDescriptionWithUICommonProperties(description, Caster);
		yield return new TooltipBrickText(description, TooltipTextType.Paragraph);
		ITooltipBrick sourceBrick = null;
		if ((bool)m_Ability?.SourceAbilityBlueprint)
		{
			sourceBrick = new TooltipBrickFeature(m_Ability.SourceAbilityBlueprint);
		}
		if (m_Ability?.SourceFact != null)
		{
			sourceBrick = new TooltipBrickFeature(m_Ability.SourceFact);
		}
		if (m_Ability?.SourceItem != null)
		{
			ItemEntity itemEntity = (ItemEntity)m_Ability.SourceItem;
			sourceBrick = new TooltipBrickIconAndName(itemEntity.Icon, itemEntity.Name);
		}
		if (sourceBrick != null)
		{
			yield return new TooltipBrickSeparator();
			yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2);
			yield return sourceBrick;
		}
	}
}
