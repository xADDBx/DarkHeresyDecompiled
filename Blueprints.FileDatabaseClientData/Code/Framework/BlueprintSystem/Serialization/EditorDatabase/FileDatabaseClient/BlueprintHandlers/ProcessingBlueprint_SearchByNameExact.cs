using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_SearchByNameExact
{
	public class RequestData
	{
		[JsonProperty("name")]
		public string? Name { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}
}
