using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_PathToStringIds
{
	public class RequestData
	{
		[JsonProperty("path")]
		public string Path { get; set; } = "";

	}

	public class ResponseData
	{
		[JsonProperty("deps")]
		public HashSet<string> Deps { get; set; } = new HashSet<string>();

	}
}
