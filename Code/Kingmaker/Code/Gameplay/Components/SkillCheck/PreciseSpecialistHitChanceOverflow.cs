using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[ComponentName("Custom/PreciseSpecialistHitChanceOverflow")]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("e7e9c3b4c1f24b9094e27e5ff0c4d0be")]
public class PreciseSpecialistHitChanceOverflow : MechanicEntityFactComponentDelegate, IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IGlobalRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
		if (evt.Type == SkillCheckType.CritSave && Rulebook.CurrentContext.First is RulePerformAttack rulePerformAttack && rulePerformAttack.Ability.IsPrecise && rulePerformAttack.InitiatorUnit == base.Owner)
		{
			RuleCalculateHitChances hitChanceRule = rulePerformAttack.RollPerformAttackRule.HitChanceRule;
			int num = hitChanceRule.RawResult - hitChanceRule.ResultHitChance;
			if (num > 0)
			{
				evt.DifficultyModifiers.Add(ModifierType.ValAdd, -1 * num, base.Fact);
			}
		}
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
	{
	}
}
