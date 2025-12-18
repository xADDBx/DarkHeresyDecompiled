using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[AllowedOn(typeof(BlueprintBehaviourTree))]
[ComponentName("AI/BehaviourTreeVariablesComponent")]
[TypeId("18d121c798404539b53e48de661abcd3")]
public class BehaviourTreeVariablesComponent : BlueprintComponent
{
	[SerializeReference]
	public List<BehaviourTreeVariableElement> Variables = new List<BehaviourTreeVariableElement>();

	public void Add(BehaviourTreeVariableElement variable)
	{
		Variables.Add(variable);
	}

	public void Remove(BehaviourTreeVariableElement variable)
	{
		Variables.Remove(variable);
	}
}
