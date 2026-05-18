using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_BaseFolder
{
	public class ResponseData
	{
		[JsonProperty("path")]
		public string? Path { get; set; }
	}
}
