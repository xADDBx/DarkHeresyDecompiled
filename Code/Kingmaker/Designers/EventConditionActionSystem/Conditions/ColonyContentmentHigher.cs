using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("fa8569250c95f9b4aa7d04c5cce9319f")]
public class ColonyContentmentHigher : Condition
{
	public int Value;

	protected override string GetConditionCaption()
	{
		return "Check if current colony contentment is higher than value";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
