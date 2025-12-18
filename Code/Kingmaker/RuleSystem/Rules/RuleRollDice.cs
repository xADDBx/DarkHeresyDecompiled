using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Utility.Random;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollDice : RulebookEvent
{
	public readonly CompositeModifiersManager Modifiers;

	public readonly DiceFormula DiceFormula;

	private int _result;

	public virtual int Result => _result;

	public RuleRollDice(IMechanicEntity initiator, DiceFormula diceFormula)
		: base(initiator)
	{
		DiceFormula = diceFormula;
		Modifiers = new CompositeModifiersManager(diceFormula.MinValue(0), diceFormula.MaxValue(0));
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Roll();
	}

	protected int Roll()
	{
		return _result = Modifiers.Apply(Randomize(DiceFormula));
	}

	private static int Randomize(DiceFormula formula)
	{
		int rolls = formula.Rolls;
		DiceType dice = formula.Dice;
		int num = 0;
		while (rolls-- > 0)
		{
			int num2 = PFStatefulRandom.RuleSystem.Range(1, dice.Sides() + 1);
			num += num2;
		}
		return num;
	}

	public static implicit operator int(RuleRollDice ruleRollDice)
	{
		return ruleRollDice?.Result ?? 0;
	}

	public override string ToString()
	{
		return _result.ToString();
	}
}
