using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/CurrentBehaviourTreeBooleanVariableGetter")]
[TypeId("eb03bbb1777b4e8c9899a89551bd2af9")]
public class CurrentBehaviourTreeBooleanVariableGetter : BoolPropertyGetter
{
	[SerializeField]
	private string VariableKey;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "'" + VariableKey + "' from BehaviourTree";
	}

	protected override bool GetBaseValue()
	{
		if (BehaviourTreeContext.Blackboard.Variables.FirstOrDefault((BlackboardVariable v) => v.Key == VariableKey) is BooleanVariable booleanVariable)
		{
			return booleanVariable.Value;
		}
		return false;
	}
}
