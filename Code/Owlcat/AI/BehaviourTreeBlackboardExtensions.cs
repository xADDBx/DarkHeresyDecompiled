using System.Collections.Generic;
using System.Reflection;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public static class BehaviourTreeBlackboardExtensions
{
	private static readonly FieldInfo BlackboardMapField = typeof(Blackboard).GetField("m_Map", BindingFlags.Instance | BindingFlags.NonPublic);

	public static bool TryGetVariable(this Blackboard blackboard, string id, out BlackboardVariable variable)
	{
		if (((Dictionary<string, BlackboardVariableWrapper>)BlackboardMapField.GetValue(blackboard)).TryGetValue(id, out var value))
		{
			variable = value.Variable;
			return true;
		}
		variable = null;
		return false;
	}

	public static EntityVariable GetAgentVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<EntityVariable>("#Agent");
	}

	public static MechanicEntityListVariable GetUnitsInCombatVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<MechanicEntityListVariable>("#UnitsInCombat");
	}

	public static MechanicEntityListVariable GetAlliesInCombatVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<MechanicEntityListVariable>("#AlliesInCombat");
	}

	public static MechanicEntityListVariable GetEnemiesInCombatVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<MechanicEntityListVariable>("#EnemiesInCombat");
	}

	public static GraphNodeListVariable GetReachableNodesVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<GraphNodeListVariable>("#ReachableGraphNodes");
	}

	public static AiAgentRuntimeInternalDataVariable GetRuntimeInternalDataVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<AiAgentRuntimeInternalDataVariable>("#RuntimeInternalData");
	}

	public static EncounterBlackboardVariable GetEncounterBlackboardVariable(this Blackboard blackboard)
	{
		return blackboard.GetVariable<EncounterBlackboardVariable>("#EncounterBlackboard");
	}
}
