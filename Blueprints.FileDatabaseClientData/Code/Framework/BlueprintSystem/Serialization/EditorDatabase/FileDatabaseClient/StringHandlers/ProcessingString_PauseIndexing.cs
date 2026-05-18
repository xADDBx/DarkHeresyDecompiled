using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_PauseIndexing
{
	public class ResponseData
	{
		[JsonProperty("ok")]
		public bool Ok => true;
	}
}
