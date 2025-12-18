using System.Collections.Generic;
using System.Text;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollD100 : RuleRollDice, IRuleRollD100, IRuleRollDice
{
	public List<int> RollHistory { get; } = new List<int>();


	public List<RerollData> Rerolls { get; } = new List<RerollData>();


	protected int? ResultOverride { get; private set; }

	public int OriginalResult => base.Result;

	public override int Result => ResultOverride ?? base.Result;

	public RuleRollD100(IMechanicEntity initiator)
		: base(initiator, new DiceFormula(1, DiceType.D100))
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (Result <= 0)
		{
			base.OnTrigger(context);
			RollHistory.Add(Result);
		}
	}

	protected void Reroll(MechanicEntityFact source)
	{
		Rerolls.Add(new RerollData(source));
		int item = Roll();
		RollHistory.Add(item);
	}

	public virtual void Override(int roll, MechanicEntityFact? source)
	{
		ResultOverride = roll;
		RollHistory.Add(roll);
		if (source != null)
		{
			Rerolls.Add(new RerollData(source));
		}
	}

	public override string ToString()
	{
		if (RollHistory == null || RollHistory.Count == 1)
		{
			return base.ToString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		bool flag2 = false;
		foreach (int item in RollHistory)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			if (item == OriginalResult && !flag2)
			{
				stringBuilder.Append("<b><u>").Append(item).Append("</u></b>");
				flag2 = true;
			}
			else
			{
				stringBuilder.Append(item);
			}
		}
		List<RerollData> rerolls = Rerolls;
		if (rerolls != null && rerolls.Count > 0)
		{
			StringBuilder stringBuilder2 = stringBuilder.Append(" [");
			List<RerollData> rerolls2 = Rerolls;
			stringBuilder2.Append(rerolls2[rerolls2.Count - 1].Source.Name).Append("]");
		}
		return stringBuilder.ToString();
	}
}
