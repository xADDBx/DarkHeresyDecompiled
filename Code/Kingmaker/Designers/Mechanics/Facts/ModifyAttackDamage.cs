using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("546c7961025e61b4fb1cad772c383a80")]
public class ModifyAttackDamage : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	[SerializeField]
	private BlueprintBuffReference m_checkedBuff;

	public float multiplierPerRank;

	public BlueprintBuff CheckedBuff => m_checkedBuff.Get();

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (!(evt.Ability != null) || evt.Ability.Blueprint.SameAbility(base.OwnerBlueprint as BlueprintAbility))
		{
			Buff buff = evt.MaybeTarget?.Buffs.GetBuff(CheckedBuff);
			if (buff != null)
			{
				int value = Mathf.RoundToInt((float)buff.Rank * multiplierPerRank * 100f);
				evt.Modifiers.Add(ModifierType.PctAdd, value, base.Fact);
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
