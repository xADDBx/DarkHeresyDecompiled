using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e66c2a26dea143018509e7d5f079aca1")]
public class TutorialTriggerUnitUsedAbility : TutorialTriggerRulebookEvent<RulePerformAbility>
{
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	protected override bool ShouldTrigger(RulePerformAbility rule)
	{
		if (rule.Context.Caster.IsPlayerEnemy)
		{
			return rule.Ability.Blueprint.SameAbility(m_Ability.Get());
		}
		return false;
	}
}
