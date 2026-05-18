using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_PathToId
{
	public class RequestData
	{
		[JsonProperty("path")]
		public string? Path { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}
}
