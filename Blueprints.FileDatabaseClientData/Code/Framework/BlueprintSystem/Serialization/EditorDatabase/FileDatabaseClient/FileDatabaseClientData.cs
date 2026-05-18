using System.Diagnostics;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient;

public class FileDatabaseClientData
{
	private static FileDatabaseClientData m_instance;

	public bool IndexingClientReadWriteLogEnabled;

	public ProcessWindowStyle BlueprintIndexingServerProcessWindowStyle;

	public static FileDatabaseClientData Instance => m_instance ?? (m_instance = new FileDatabaseClientData());
}
