using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_GetEntitiesByBlueprint
{
	public class RequestData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("entities")]
		public List<string> Entities { get; set; }
	}
}
