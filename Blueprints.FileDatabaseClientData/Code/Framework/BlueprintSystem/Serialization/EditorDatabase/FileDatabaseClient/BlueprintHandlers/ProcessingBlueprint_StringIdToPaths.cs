using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_StringIdToPaths
{
	public class RequestData
	{
		[JsonProperty("id")]
		public string Id { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("paths")]
		public HashSet<string> Paths { get; set; } = new HashSet<string>();

	}
}
