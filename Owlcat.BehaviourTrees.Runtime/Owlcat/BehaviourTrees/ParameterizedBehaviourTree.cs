using System;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class ParameterizedBehaviourTree
{
	public BehaviourTreeReference BehaviourTree;

	public VariableMappingContainer MappingContainer;

	[SerializeReference]
	public BehaviourTreeVariableElement[] Variables;
}
