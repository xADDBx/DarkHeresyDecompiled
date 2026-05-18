using System;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
[Obsolete("WH")]
public class ContextDiceValue
{
	[HideInInspector]
	public DiceType DiceType;

	[HideInInspector]
	public ContextValue DiceCountValue;

	[HideInInspector]
	public ContextValue BonusValue;

	public bool IsVariable => DiceType > DiceType.One;

	public int Calculate(IEvalContext context)
	{
		return ContextValueHelper.CalculateDiceValue(DiceType, DiceCountValue, BonusValue, context);
	}

	public override string ToString()
	{
		bool num = DiceCountValue.ValueType == ContextValueType.Const && DiceCountValue.Value == 0;
		bool flag = BonusValue.ValueType == ContextValueType.Const && BonusValue.Value == 0;
		if (!num)
		{
			if (!flag)
			{
				return $"{DiceCountValue}{DiceType}+{BonusValue}";
			}
			return $"{DiceCountValue}{DiceType}";
		}
		if (!flag)
		{
			return BonusValue.ToString();
		}
		return "0";
	}
}
