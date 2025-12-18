using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/CurrentBehaviourTreeIntegerVariableGetter")]
[TypeId("e256e5f03e184478bbae6bd899ca8ae5")]
public class CurrentBehaviourTreeIntegerVariableGetter : IntPropertyGetter
{
	[SerializeField]
	private string VariableKey;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "'" + VariableKey + "' from BehaviourTree";
	}

	protected override int GetBaseValue()
	{
		if (!(BehaviourTreeContext.Blackboard.Variables.FirstOrDefault((BlackboardVariable v) => v.Key == VariableKey) is IntegerVariable integerVariable))
		{
			return 0;
		}
		return integerVariable.Value;
	}
}
