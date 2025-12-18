using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public abstract class BaseUseAbilityLogThread : LogThreadBase
{
	protected void HandleUseAbility(AbilityData ability, [CanBeNull] RulePerformAbility rule)
	{
		if (ability.Blueprint.DisableLog)
		{
			return;
		}
		bool flag = !ability.Name.IsNullOrEmpty();
		ItemEntity sourceItem = ability.SourceItem;
		bool flag2 = sourceItem != null && !sourceItem.Name.IsNullOrEmpty() && sourceItem.Name != ability.Name;
		if (!flag && !flag2)
		{
			return;
		}
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (rule != null && rule.Context.ExecutionFromPsychicPhenomena)
			{
				GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Context.Caster;
				GameLogContext.Text = rule.Context.AbilityBlueprint.Name;
				CombatLogMessage message = LogThreadBase.Strings.PerilsOfTheWarp.CreateCombatLogMessage();
				TooltipBaseTemplate template = CombatLogTooltipService.CreateTooltipTemplateAbility(ability);
				AddMessage(new CombatLogMessage(message, template, hasTooltip: true, rule.Context.Caster));
				return;
			}
			MechanicEntity mechanicEntity = rule?.ConcreteInitiator ?? ability.Caster;
			MechanicEntity mechanicEntity2 = rule?.AbilityTarget.Entity;
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)mechanicEntity;
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((mechanicEntity2 == mechanicEntity) ? null : mechanicEntity2);
			GameLogContext.Text = ((flag && flag2) ? (sourceItem.Name + " / " + ability.Name) : (flag ? ability.Name : sourceItem.Name));
			GameLogContext.Description = ability.Description;
			if (sourceItem != null)
			{
				ItemEntity itemEntity = ItemsEntityFactory.CreateItemCopy(ability.SourceItem, 1);
				GameLogContext.Tooltip = itemEntity;
				CombatLogMessage message2 = LogThreadBase.Strings.UseItem.CreateCombatLogMessage();
				TooltipBaseTemplate template2 = CombatLogTooltipService.CreateTooltipTemplateItemBlueprint(itemEntity.Blueprint);
				AddMessage(new CombatLogMessage(message2, template2, hasTooltip: true, mechanicEntity));
			}
			else
			{
				GameLogContext.Tooltip = ability;
				CombatLogMessage message3 = ((GameLogContext.TargetEntity.Value != null) ? LogThreadBase.Strings.UseAbilityOnTarget : LogThreadBase.Strings.UseAbility).CreateCombatLogMessage();
				TooltipBaseTemplate template3 = CombatLogTooltipService.CreateTooltipTemplateAbility(ability);
				AddMessage(new CombatLogMessage(message3, template3, hasTooltip: true, mechanicEntity));
			}
			LogThreadBase.IsPreviousMessageUseSomething = true;
		}
	}
}
