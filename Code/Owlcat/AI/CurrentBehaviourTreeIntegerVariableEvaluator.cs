using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/CurrentBehaviourTreeIntegerVariableEvaluator")]
[TypeId("da9d1a9c7336408ea0de5c62e9e7a42a")]
public class CurrentBehaviourTreeIntegerVariableEvaluator : IntEvaluator
{
	[SerializeField]
	private string VariableKey;

	public override string GetCaption()
	{
		return "'" + VariableKey + "' from BehaviourTree";
	}

	protected override int GetValueInternal()
	{
		if (!(BehaviourTreeContext.Blackboard.Variables.FirstOrDefault((BlackboardVariable v) => v.Key == VariableKey) is IntegerVariable integerVariable))
		{
			return 0;
		}
		return integerVariable.Value;
	}
}
