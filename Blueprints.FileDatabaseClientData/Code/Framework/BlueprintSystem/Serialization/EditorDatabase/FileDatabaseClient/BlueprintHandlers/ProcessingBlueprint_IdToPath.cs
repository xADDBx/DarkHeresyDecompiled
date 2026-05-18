using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_IdToPath
{
	public class RequestData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("path")]
		public string? Path { get; set; }
	}
}
