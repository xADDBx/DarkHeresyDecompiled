using System;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Buffs;

public struct BuffDuration
{
	public readonly Rounds? Rounds;

	public readonly BuffEndCondition EndCondition;

	public readonly BuffExpireMoment ExpireMoment;

	public readonly bool IsPermanent
	{
		get
		{
			if (Rounds.HasValue)
			{
				Rounds? rounds = Rounds;
				Rounds infinity = Kingmaker.Utility.Rounds.Infinity;
				if (!rounds.HasValue)
				{
					return false;
				}
				if (!rounds.HasValue)
				{
					return true;
				}
				return rounds.GetValueOrDefault() == infinity;
			}
			return true;
		}
	}

	public BuffDuration(Rounds? rounds = null, BuffEndCondition endCondition = BuffEndCondition.RemainAfterCombat, BuffExpireMoment expireMoment = BuffExpireMoment.TurnStart)
	{
		Rounds = rounds;
		EndCondition = endCondition;
		ExpireMoment = expireMoment;
	}

	public BuffDuration(TimeSpan timeSpan)
	{
		Rounds = timeSpan.ToRounds();
		EndCondition = BuffEndCondition.RemainAfterCombat;
		ExpireMoment = BuffExpireMoment.TurnStart;
	}

	public BuffDuration(Buff buff)
	{
		Rounds = buff.DurationInRounds.Rounds();
		EndCondition = buff.EndCondition;
		ExpireMoment = BuffExpireMoment.TurnStart;
	}

	public static implicit operator BuffDuration(Rounds? rounds)
	{
		return new BuffDuration(rounds);
	}

	public static implicit operator BuffDuration(TimeSpan timeSpan)
	{
		return new BuffDuration(timeSpan);
	}

	public static implicit operator BuffDuration(BuffEndCondition endCondition)
	{
		return new BuffDuration(null, endCondition);
	}
}
