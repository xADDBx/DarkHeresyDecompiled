using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpToggleAbility : TooltipBaseTemplate
{
	private readonly ToggleAbility m_Ability;

	private readonly string m_Name;

	private readonly Sprite m_Icon;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly MechanicEntity m_Caster;

	public readonly BlueprintToggleAbility BlueprintAbility;

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

	public TooltipTemplateLevelUpToggleAbility(BlueprintToggleAbility ability, MechanicEntity caster = null, LevelUpManager levelUpManager = null)
	{
		if (ability != null)
		{
			m_Caster = caster;
			m_LevelUpManager = levelUpManager;
			m_Name = ability.Name;
			m_Icon = ability.Icon;
			BlueprintAbility = ability;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<string> values = BlueprintAbility.AbilityModifierTags.Select((BpRef<BlueprintAbilityTag> tag) => tag.Blueprint.Name.Text).ToList();
		yield return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(new TextValueElement(m_Name), null, new TextValueElement(string.Join(", ", values)), null, m_Icon));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
			string text = m_Ability?.Description ?? BlueprintAbility.Description;
			yield return new BrickTextVM(text, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, Caster);
			ITooltipBrick sourceBrick = null;
			if (m_Ability?.SourceAbilityBlueprint != null)
			{
				sourceBrick = new BrickFeatureVM(m_Ability.SourceAbilityBlueprint);
			}
			if (m_Ability?.SourceFact != null)
			{
				sourceBrick = new BrickFeatureVM(m_Ability.SourceFact);
			}
			if (m_Ability?.SourceItem != null)
			{
				ItemEntity itemEntity = (ItemEntity)m_Ability.SourceItem;
				sourceBrick = new BrickIconAndNameVM(itemEntity.Name, itemEntity.Icon);
			}
			if (sourceBrick != null)
			{
				yield return new BrickSeparatorVM();
				yield return new BrickTitleVM(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2);
				yield return sourceBrick;
			}
		}
	}
}
