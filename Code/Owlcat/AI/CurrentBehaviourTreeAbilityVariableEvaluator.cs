using System;
using System.Collections.ObjectModel;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/CurrentBehaviourTreeAbilityVariableEvaluator")]
[TypeId("4bde99a9cc4e9014893dbd4f25a0fb6e")]
public class CurrentBehaviourTreeAbilityVariableEvaluator : AbilityEvaluator
{
	[SerializeField]
	private string VariableKey;

	public override string GetCaption()
	{
		return "'" + VariableKey + "' from BehaviourTree";
	}

	protected override AbilityData GetValueInternal()
	{
		ReadOnlyCollection<BlackboardVariable> variables = BehaviourTreeContext.Blackboard.Variables;
		for (int i = 0; i < variables.Count; i++)
		{
			if (variables[i] is AbilityVariable abilityVariable && abilityVariable.Key == VariableKey)
			{
				return abilityVariable.Value;
			}
		}
		return null;
	}
}
