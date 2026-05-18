using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateToggleAbility : TooltipBaseTemplate
{
	private const float BrickBigSpaceHeight = 25f;

	private readonly ToggleAbility m_Ability;

	private readonly MechanicEntity m_Caster;

	private readonly string m_Name;

	private readonly Sprite m_Icon;

	private readonly string m_Description;

	private bool m_HasCasterRestrictions;

	private bool m_CasterRestrictionsPassed;

	private IReadOnlyList<BlueprintAbilityModifier> m_AppliedModifiers;

	private IEnumerable<StatType> m_ScalingStats;

	public readonly BlueprintToggleAbility BlueprintAbility;

	public TooltipTemplateToggleAbility(ToggleAbility ability, MechanicEntity caster = null)
	{
		if (ability != null)
		{
			ContentSpacing = 0f;
			m_Caster = caster ?? ability.Caster;
			m_Icon = ability.Icon;
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				m_Name = ability.Name;
				m_Description = ability.Description;
			}
			BlueprintAbility = ability.Blueprint;
			m_Ability = ability;
		}
	}

	public TooltipTemplateToggleAbility(BlueprintToggleAbility ability, MechanicEntity caster = null)
	{
		if (ability != null)
		{
			ContentSpacing = 0f;
			m_Caster = caster;
			m_Icon = ability.Icon;
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				m_Name = ability.Name;
				m_Description = ability.Description;
			}
			BlueprintAbility = ability;
		}
	}

	public override void Prepare(TooltipTemplateType type)
	{
		m_HasCasterRestrictions = BlueprintAbility.HasCasterRestrictions(m_Caster, out m_CasterRestrictionsPassed);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Ability != null)
		{
			return GetAbilityHeader();
		}
		return GetBlueprintAbilityHeader();
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddScalingCharacteristics(list);
		AddDescription(list);
		AddAppliedModifiers(list);
		if (m_HasCasterRestrictions && m_CasterRestrictionsPassed)
		{
			ITooltipBrick casterRestrictionsBrick = GetCasterRestrictionsBrick();
			if (casterRestrictionsBrick != null)
			{
				list.Add(new BrickSpaceVM(25f));
				list.Add(casterRestrictionsBrick);
			}
		}
		AddSource(list);
		return list;
	}

	private IEnumerable<ITooltipBrick> GetAbilityHeader()
	{
		if (m_CasterRestrictionsPassed)
		{
			ITooltipBrick casterRestrictionsBrick = GetCasterRestrictionsBrick();
			if (casterRestrictionsBrick != null)
			{
				yield return casterRestrictionsBrick;
			}
		}
		yield return new BrickAbilityHeaderVM(m_Name, UIStrings.Instance.Tooltips.ToggleAbilityType, m_Icon).SetToggleState(m_Ability.Enabled);
	}

	private IEnumerable<ITooltipBrick> GetBlueprintAbilityHeader()
	{
		yield return new BrickEntityHeaderVM(m_Name, m_Icon, hasUpgrade: false);
		yield return new BrickSeparatorVM();
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		string description = m_Description;
		bricks.Add(new BrickSpaceVM(25f));
		bricks.Add(new BrickFormattedDescriptionVM(description, m_Caster));
	}

	private void AddScalingCharacteristics(List<ITooltipBrick> bricks)
	{
		if (m_ScalingStats == null)
		{
			m_ScalingStats = BlueprintAbility.GetScalingStats();
		}
		if (m_Caster != null && m_ScalingStats != null && m_ScalingStats.Any())
		{
			bricks.Add(new BrickAbilityScalingStatsVM(m_ScalingStats, m_Caster));
		}
	}

	private void AddAppliedModifiers(List<ITooltipBrick> bricks)
	{
		if (m_AppliedModifiers == null)
		{
			m_AppliedModifiers = BlueprintAbility.GetAppliedModifiers(m_Caster);
		}
		if (m_AppliedModifiers != null && m_AppliedModifiers.Count >= 1)
		{
			bricks.Add(new BrickAbilityModifiersVM(m_AppliedModifiers, null, m_Caster));
		}
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (m_Ability != null)
		{
			if ((bool)m_Ability.SourceAbilityBlueprint)
			{
				AddSourceTitle(bricks);
				bricks.Add(new BrickFeatureVM(m_Ability.SourceAbilityBlueprint));
			}
			else if (m_Ability?.SourceFact != null)
			{
				AddSourceTitle(bricks);
				bricks.Add(new BrickFeatureVM(m_Ability.SourceFact));
			}
			else if (m_Ability.SourceItem != null)
			{
				ItemEntity itemEntity = (ItemEntity)m_Ability.SourceItem;
				AddSourceTitle(bricks);
				bricks.Add(new BrickIconAndNameVM(itemEntity.Name, itemEntity.Icon));
			}
		}
		static void AddSourceTitle(List<ITooltipBrick> result)
		{
			result.Add(new BrickSeparatorVM());
			result.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2));
		}
	}

	private ITooltipBrick GetCasterRestrictionsBrick()
	{
		if (BlueprintAbility == null || m_Caster == null || !m_HasCasterRestrictions)
		{
			return null;
		}
		return new BrickAbilityRestrictionsVM(BlueprintAbility, m_Caster);
	}
}
