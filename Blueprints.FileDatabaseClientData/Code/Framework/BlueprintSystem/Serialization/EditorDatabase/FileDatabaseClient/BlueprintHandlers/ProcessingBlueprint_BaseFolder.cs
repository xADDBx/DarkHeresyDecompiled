using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_BaseFolder
{
	public class ResponseData
	{
		[JsonProperty("path")]
		public string? Path { get; set; }
	}
}
