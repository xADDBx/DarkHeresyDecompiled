using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Damage;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("Damage/AddDamageTypeImmunity")]
[TypeId("245250259ef6425787b71c6fa1b493d5")]
public class AddDamageTypeImmunity : MechanicEntityFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber
{
	[EnumFlagsAsButtons]
	public DamageTypeMask Types;

	[SerializeField]
	private ActionList m_ActionsOnImmunity;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (Types.Contains(evt.DamageType))
		{
			evt.Modifiers.Add(ModifierType.PctMul_Extra, 0, base.Fact, ModifierDescriptor.Immunity);
			base.Fact.RunActionInContext(m_ActionsOnImmunity, base.Owner.ToITargetWrapper());
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
