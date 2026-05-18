using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_ResumeIndexing
{
	public class ResponseData
	{
		[JsonProperty("ok")]
		public bool Ok => true;
	}
}
