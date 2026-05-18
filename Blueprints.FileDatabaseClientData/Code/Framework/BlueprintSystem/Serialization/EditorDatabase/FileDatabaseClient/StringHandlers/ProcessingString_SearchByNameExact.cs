using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_SearchByNameExact
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
