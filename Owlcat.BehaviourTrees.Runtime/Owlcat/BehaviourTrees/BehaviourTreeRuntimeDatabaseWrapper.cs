namespace Owlcat.BehaviourTrees;

public class BehaviourTreeRuntimeDatabaseWrapper
{
	private readonly IBehaviourTreeRuntimeDatabase m_Database;

	public static BehaviourTreeRuntimeDatabaseWrapper Instance { get; private set; }

	public static void Initialize(IBehaviourTreeRuntimeDatabase database)
	{
		Instance = new BehaviourTreeRuntimeDatabaseWrapper(database);
	}

	private BehaviourTreeRuntimeDatabaseWrapper(IBehaviourTreeRuntimeDatabase database)
	{
		m_Database = database;
	}

	public BehaviourTreeSerializableData LoadById(string assetGuid)
	{
		return m_Database.LoadById(assetGuid);
	}
}
