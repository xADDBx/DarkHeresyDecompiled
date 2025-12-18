using Kingmaker.Blueprints.JsonSystem.EditorDatabase;

namespace Owlcat.BehaviourTrees;

public class BlueprintBehaviourTreeRuntimeDatabaseImpl : IBehaviourTreeRuntimeDatabase
{
	public BehaviourTreeSerializableData LoadById(string id)
	{
		return BlueprintsDatabase.LoadById<BlueprintBehaviourTree>(id)?.Data;
	}
}
