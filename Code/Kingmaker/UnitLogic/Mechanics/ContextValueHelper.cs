using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UnitLogic.Mechanics;

public static class ContextValueHelper
{
	public static int CalculateDiceValue(DiceType diceType, ContextValue diceCountValue, ContextValue bonusValue, IEvalContext context)
	{
		int rollsCount = diceCountValue.Calculate(context);
		int num = bonusValue.Calculate(context);
		MechanicEntity mechanicEntity = context.Caster ?? context.Owner;
		int num2;
		if (mechanicEntity != null)
		{
			RuleRollDice rule = new RuleRollDice(mechanicEntity, new DiceFormula(rollsCount, diceType));
			num2 = EvalContext.Current.TriggerRule(rule).Result;
		}
		else
		{
			PFLog.Default.Error("Caster and owner is missing");
			num2 = 0;
		}
		return num2 + num;
	}
}
