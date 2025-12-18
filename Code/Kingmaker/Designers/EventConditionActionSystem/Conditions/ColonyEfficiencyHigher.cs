using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("99e0c62f06ea0024199cbb2ba69a615e")]
public class ColonyEfficiencyHigher : Condition
{
	public int Value;

	protected override string GetConditionCaption()
	{
		return "Check if current colony efficiency is higher than value";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
