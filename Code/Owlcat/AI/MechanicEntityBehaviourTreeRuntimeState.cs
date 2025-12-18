using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.BehaviourTrees;
using Owlcat.EntityBlackboard;

namespace Owlcat.AI;

public class MechanicEntityBehaviourTreeRuntimeState : IRuntimeEntityBlackboard
{
	private readonly List<RuntimeVariable> m_Variables;

	public IEnumerable<RuntimeVariable> Variables => m_Variables;

	public MechanicEntityBehaviourTreeRuntimeState(BehaviourTreeSerializableData data, Blackboard blackboard, BehaviourTree behaviourTree)
	{
		m_Variables = GetStorableVariables(data, blackboard).Concat(GetLimitedEntryNodesState(data, behaviourTree)).ToList();
	}

	public static void Restore(IRuntimeEntityBlackboard runtimeEntityBlackboard, Blackboard blackboard, BehaviourTree behaviourTree)
	{
		if (runtimeEntityBlackboard != null)
		{
			SetStorableVariables(runtimeEntityBlackboard, blackboard);
			SetLimitedEntryNodesState(runtimeEntityBlackboard, behaviourTree);
		}
	}

	private IEnumerable<RuntimeVariable> GetStorableVariables(BehaviourTreeSerializableData data, Blackboard blackboard)
	{
		foreach (BehaviourTreeVariableElement variable in data.Variables)
		{
			Type type = variable.GetType();
			if (type == typeof(Owlcat.BehaviourTrees.IntegerVariableElement))
			{
				yield return new Owlcat.EntityBlackboard.IntegerVariable
				{
					Key = variable.Id,
					Value = blackboard.GetVariable<Owlcat.BehaviourTrees.IntegerVariable>(variable.Id).Value
				};
			}
			else if (type == typeof(Owlcat.BehaviourTrees.BooleanVariableElement))
			{
				yield return new Owlcat.EntityBlackboard.IntegerVariable
				{
					Key = variable.Id,
					Value = (blackboard.GetVariable<BooleanVariable>(variable.Id).Value ? 1 : 0)
				};
			}
			else if (type == typeof(FloatVariableElement))
			{
				yield return new Owlcat.EntityBlackboard.FloatVariable
				{
					Key = variable.Id,
					Value = blackboard.GetVariable<Owlcat.BehaviourTrees.FloatVariable>(variable.Id).Value
				};
			}
			else if (type == typeof(PositionVariableElement))
			{
				yield return new Vector3Variable
				{
					Key = variable.Id,
					Value = blackboard.GetVariable<PositionVariable>(variable.Id).Value
				};
			}
		}
	}

	private IEnumerable<Owlcat.EntityBlackboard.IntegerVariable> GetLimitedEntryNodesState(BehaviourTreeSerializableData data, BehaviourTree behaviourTree)
	{
		foreach (BehaviourTreeNodeElement nodeElement in data.Nodes)
		{
			if (nodeElement is LimitedEntryNodeElement && behaviourTree.Nodes.FindOrDefault((BehaviourTreeNode n) => n.NodeElement.Id == nodeElement.Id) is LimitedEntryNode limitedEntryNode)
			{
				yield return new Owlcat.EntityBlackboard.IntegerVariable
				{
					Key = nodeElement.Id,
					Value = limitedEntryNode.EntryCount
				};
			}
		}
	}

	private static void SetStorableVariables(IRuntimeEntityBlackboard runtimeEntityBlackboard, Blackboard blackboard)
	{
		foreach (RuntimeVariable variable in runtimeEntityBlackboard.Variables)
		{
			string key = variable.Key;
			BlackboardVariable blackboardVariable = null;
			try
			{
				blackboardVariable = blackboard.GetVariable(key);
			}
			catch (Exception)
			{
			}
			if (blackboardVariable == null)
			{
				continue;
			}
			if (!(blackboardVariable is Owlcat.BehaviourTrees.IntegerVariable integerVariable))
			{
				if (!(blackboardVariable is BooleanVariable booleanVariable))
				{
					if (!(blackboardVariable is Owlcat.BehaviourTrees.FloatVariable floatVariable))
					{
						if (blackboardVariable is PositionVariable positionVariable && variable is Vector3Variable vector3Variable)
						{
							positionVariable.Value = vector3Variable.Value;
						}
					}
					else if (variable is Owlcat.EntityBlackboard.FloatVariable floatVariable2)
					{
						floatVariable.Value = floatVariable2.Value;
					}
				}
				else if (variable is Owlcat.EntityBlackboard.IntegerVariable integerVariable2)
				{
					booleanVariable.Value = integerVariable2.Value != 0;
				}
			}
			else if (variable is Owlcat.EntityBlackboard.IntegerVariable integerVariable3)
			{
				integerVariable.Value = integerVariable3.Value;
			}
		}
	}

	private static void SetLimitedEntryNodesState(IRuntimeEntityBlackboard runtimeEntityBlackboard, BehaviourTree behaviourTree)
	{
		foreach (RuntimeVariable runtimeVariable in runtimeEntityBlackboard.Variables)
		{
			if (runtimeVariable is Owlcat.EntityBlackboard.IntegerVariable integerVariable && behaviourTree.Nodes.FindOrDefault((BehaviourTreeNode n) => n.NodeElement.Id == runtimeVariable.Key) is LimitedEntryNode obj)
			{
				PropertyInfo property = typeof(LimitedEntryNode).GetProperty("EntryCount");
				if (property != null)
				{
					property.SetValue(obj, integerVariable.Value);
				}
			}
		}
	}
}
