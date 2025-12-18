using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.RuleSystem.Rules.Disperse;

public sealed class RuleDisperseDamage : RulebookTargetEvent
{
	private readonly MechanicEntity[] _disperseTargets;

	private readonly RolledDamage _rolledDamage;

	private readonly MechanicEntityFact _sourceFact;

	private readonly ContextValueModifierWithType _damageModifyBeforeDispersing;

	public RuleDisperseDamage(MechanicEntity initiator, MechanicEntity target, MechanicEntity[] disperseTargets, RolledDamage rolledDamage, MechanicEntityFact sourceFact, ContextValueModifierWithType damageModifyBeforeDispersing)
		: base(initiator, target)
	{
		_disperseTargets = disperseTargets;
		_rolledDamage = rolledDamage;
		_sourceFact = sourceFact;
		_damageModifyBeforeDispersing = damageModifyBeforeDispersing;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (_disperseTargets.Length != 0)
		{
			IntermediateDamage intermediateDamage = _rolledDamage.CopyAsIntermediateDamage();
			_damageModifyBeforeDispersing.TryApply(intermediateDamage.Modifiers, _sourceFact, ModifierDescriptor.None);
			intermediateDamage.Modifiers.Add(ModifierType.PctMul_Extra, 100 / _disperseTargets.Length, _sourceFact);
			intermediateDamage.CalculatedValue = _rolledDamage.ResultDamageValue;
			intermediateDamage.MarkCalculated();
			MechanicEntity[] disperseTargets = _disperseTargets;
			foreach (MechanicEntity target in disperseTargets)
			{
				Rulebook.Trigger(new RuleDealDamage(base.Initiator, target, intermediateDamage.Copy())
				{
					IsDispersedDamage = true
				});
			}
		}
	}
}
