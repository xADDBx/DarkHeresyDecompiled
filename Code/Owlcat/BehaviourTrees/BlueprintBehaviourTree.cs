using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.EntityBlackboard;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[ComponentName("AI/BlueprintBehaviourTree")]
[TypeId("75e5d7b9bf634929965764b8855ac31f")]
public class BlueprintBehaviourTree : BlueprintScriptableObject, IBehaviourTreeOwnerAsset
{
	public BehaviourTreeSerializableData Data;

	public new string AssetGuid => base.AssetGuid;

	private bool IsJustCreated
	{
		get
		{
			if (Data.Variables.Empty())
			{
				return Data.Nodes.Empty();
			}
			return false;
		}
	}

	public string GetTitle()
	{
		return name;
	}

	public BehaviourTreeSerializableData GetData()
	{
		return Data;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (Data == null)
		{
			Data = new BehaviourTreeSerializableData();
		}
		Data.SetOwnerAsset(this);
		bool isJustCreated = IsJustCreated;
		EnsureNecessaryVariables(Data);
		if (isJustCreated)
		{
			EnsureAuxiliaryVariables(Data);
		}
		EnsureEncounterBlackboardVariable(this, Data);
	}

	private static void EnsureNecessaryVariables(BehaviourTreeSerializableData data)
	{
		EnsureAgentVariable(data);
		EnsureUnitsInCombatVariable(data);
		EnsureAlliesInCombatVariable(data);
		EnsureEnemiesInCombatVariable(data);
		EnsureReachableNodesVariable(data);
		EnsureRuntimeInternalDataVariable(data);
	}

	private static void EnsureAuxiliaryVariables(BehaviourTreeSerializableData data)
	{
		EnsureTargetVariable(data);
		EnsureAlliesVariable(data);
		EnsureEnemiesVariable(data);
		EnsureSelectedAbilityVariable(data);
		EnsureSelectedGraphNodeVariable(data);
	}

	private static void EnsureAgentVariable(BehaviourTreeSerializableData data)
	{
		BehaviourTreeVariableElement behaviourTreeVariableElement = data.Variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.name == "#Agent");
		if (behaviourTreeVariableElement != null)
		{
			behaviourTreeVariableElement.IsSettable = false;
			return;
		}
		data.AddVariable(new EntityVariableElement
		{
			name = "#Agent",
			Key = "#Agent",
			IsSettable = false
		});
	}

	private static void EnsureUnitsInCombatVariable(BehaviourTreeSerializableData data)
	{
		BehaviourTreeVariableElement behaviourTreeVariableElement = data.Variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.name == "#UnitsInCombat");
		if (behaviourTreeVariableElement != null)
		{
			behaviourTreeVariableElement.IsSettable = false;
			return;
		}
		data.AddVariable(new MechanicEntityListVariableElement
		{
			name = "#UnitsInCombat",
			Key = "#UnitsInCombat",
			IsSettable = false
		});
	}

	private static void EnsureAlliesInCombatVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "#AlliesInCombat"))
		{
			data.AddVariable(new MechanicEntityListVariableElement
			{
				name = "#AlliesInCombat",
				Key = "#AlliesInCombat"
			});
		}
	}

	private static void EnsureEnemiesInCombatVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "#EnemiesInCombat"))
		{
			data.AddVariable(new MechanicEntityListVariableElement
			{
				name = "#EnemiesInCombat",
				Key = "#EnemiesInCombat"
			});
		}
	}

	private static void EnsureReachableNodesVariable(BehaviourTreeSerializableData data)
	{
		BehaviourTreeVariableElement behaviourTreeVariableElement = data.Variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.name == "#ReachableGraphNodes");
		if (behaviourTreeVariableElement != null)
		{
			behaviourTreeVariableElement.IsSettable = false;
			return;
		}
		data.AddVariable(new GraphNodeListVariableElement
		{
			name = "#ReachableGraphNodes",
			Key = "#ReachableGraphNodes",
			IsSettable = false
		});
	}

	private static void EnsureRuntimeInternalDataVariable(BehaviourTreeSerializableData data)
	{
		BehaviourTreeVariableElement behaviourTreeVariableElement = data.Variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.name == "#RuntimeInternalData");
		if (behaviourTreeVariableElement != null)
		{
			behaviourTreeVariableElement.IsSettable = false;
			return;
		}
		data.AddVariable(new AiAgentRuntimeInternalDataVariableElement
		{
			name = "#RuntimeInternalData",
			Key = "#RuntimeInternalData",
			IsSettable = false
		});
	}

	private static void EnsureEncounterBlackboardVariable(BlueprintBehaviourTree blueprintBehaviourTree, BehaviourTreeSerializableData data)
	{
		EntityBlackboardComponent entityBlackboardComponent = null;
		if (blueprintBehaviourTree.TryGetComponent<EncounterBlackboard>(out var component) && component.Encounter != null && component.Encounter.TryGetComponent<EntityBlackboardComponent>(out var component2))
		{
			entityBlackboardComponent = component2;
		}
		BehaviourTreeVariableElement behaviourTreeVariableElement = data.Variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.name == "#EncounterBlackboard");
		if (behaviourTreeVariableElement != null)
		{
			data.RemoveVariable(behaviourTreeVariableElement);
		}
		if (entityBlackboardComponent != null)
		{
			data.AddVariable(new EncounterBlackboardVariableElement(component.Encounter.Reference())
			{
				name = "#EncounterBlackboard",
				Key = "#EncounterBlackboard",
				IsSettable = false
			});
		}
	}

	private static void EnsureTargetVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "Target"))
		{
			data.AddVariable(new EntityVariableElement
			{
				name = "Target",
				Key = "Target"
			});
		}
	}

	private static void EnsureAlliesVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "Allies"))
		{
			data.AddVariable(new MechanicEntityListVariableElement
			{
				name = "Allies",
				Key = "Allies"
			});
		}
	}

	private static void EnsureEnemiesVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "Enemies"))
		{
			data.AddVariable(new MechanicEntityListVariableElement
			{
				name = "Enemies",
				Key = "Enemies"
			});
		}
	}

	private static void EnsureSelectedAbilityVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "SelectedAbility"))
		{
			data.AddVariable(new AbilityVariableElement
			{
				name = "SelectedAbility",
				Key = "SelectedAbility"
			});
		}
	}

	private static void EnsureSelectedGraphNodeVariable(BehaviourTreeSerializableData data)
	{
		if (!data.Variables.Contains((BehaviourTreeVariableElement v) => v.name == "SelectedGraphNode"))
		{
			data.AddVariable(new GraphNodeVariableElement
			{
				name = "SelectedGraphNode",
				Key = "SelectedGraphNode"
			});
		}
	}
}
