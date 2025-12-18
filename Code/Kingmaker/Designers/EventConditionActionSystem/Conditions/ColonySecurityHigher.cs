using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("09eb8c915fdc4824a8f21957ca8f6152")]
public class ColonySecurityHigher : Condition
{
	public int Value;

	protected override string GetConditionCaption()
	{
		return "Check if current colony security is higher than value";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
