using System;
using Kingmaker.Framework;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextDurationValue
{
	public ContextValue RoundsValue;

	public Rounds Calculate(IEvalContext context)
	{
		return RoundsValue.Calculate(context).Rounds();
	}

	public override string ToString()
	{
		return RoundsValue.ToString() + " Rounds";
	}
}
