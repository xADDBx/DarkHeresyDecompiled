using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpToggleAbility : TooltipBaseTemplate
{
	private ToggleAbility m_Ability;

	public readonly BlueprintToggleAbility BlueprintAbility;

	private readonly string m_Name;

	private readonly Sprite m_Icon;

	private readonly string m_Description;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly MechanicEntity m_Caster;

	private MechanicEntity Caster
	{
		get
		{
			object obj = m_Caster;
			if (obj == null)
			{
				LevelUpManager levelUpManager = m_LevelUpManager;
				if (levelUpManager == null)
				{
					return null;
				}
				obj = levelUpManager.PreviewUnit;
			}
			return (MechanicEntity)obj;
		}
	}

	public TooltipTemplateLevelUpToggleAbility(ToggleAbility ability, MechanicEntity caster = null, LevelUpManager levelUpManager = null)
	{
		if (ability != null)
		{
			m_Name = ability.Name;
			m_Icon = ability.Icon;
			m_Description = ability.Description;
			m_Caster = caster;
			BlueprintAbility = ability.Blueprint;
			m_Ability = ability;
			m_LevelUpManager = levelUpManager;
		}
	}

	public TooltipTemplateLevelUpToggleAbility(BlueprintToggleAbility ability, MechanicEntity caster = null, LevelUpManager levelUpManager = null)
	{
		if (ability != null)
		{
			m_Name = ability.Name;
			m_Icon = ability.Icon;
			m_Description = ability.Description;
			m_Caster = caster;
			BlueprintAbility = ability;
			m_LevelUpManager = levelUpManager;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<string> values = BlueprintAbility.AbilityModifierTags.Select((BpRef<BlueprintAbilityTag> tag) => tag.Blueprint.Name.Text).ToList();
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(m_Name, null, null, string.Join(", ", values), null, null, m_Icon));
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
