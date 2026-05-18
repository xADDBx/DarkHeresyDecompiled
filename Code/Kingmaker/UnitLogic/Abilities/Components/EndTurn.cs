using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[ComponentName("Combat/EndTurn")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("224c1eb24ed98e942ae15b9e10e62724")]
public class EndTurn : BlueprintComponent, IAbilityOnCastLogic
{
	public bool clearMPInsteadOfEndingTurn;

	public ConditionsChecker Condition;

	[SerializeField]
	private BlueprintBuffReference m_BuffToCaster;

	public BlueprintBuff BuffToCaster => m_BuffToCaster?.Get();

	public void OnCast(AbilityExecutionContext context)
	{
		using (EvalContext.PushContext(context, context.Caster))
		{
			if (Condition != null && !Condition.Check() && Condition.HasConditions)
			{
				return;
			}
		}
		context.Caster.Buffs.Add(BuffToCaster, context);
		if (clearMPInsteadOfEndingTurn)
		{
			PartUnitCombatState combatStateOptional = context.Caster.GetCombatStateOptional();
			if (combatStateOptional != null && !(context.MaybeCaster?.Features.DoNotResetMovementPointsOnAttacks) && !context.FreeAction)
			{
				if (combatStateOptional.SaveMPAfterUsingNextAbility)
				{
					combatStateOptional.SaveMPAfterUsingNextAbility = false;
				}
				else
				{
					combatStateOptional.SpendMovementsPointsAll();
				}
			}
		}
		else if (Game.Instance.Controllers.TurnController.CurrentUnit == context.Caster)
		{
			Game.Instance.Controllers.TurnController.RequestEndTurn();
		}
	}
}
