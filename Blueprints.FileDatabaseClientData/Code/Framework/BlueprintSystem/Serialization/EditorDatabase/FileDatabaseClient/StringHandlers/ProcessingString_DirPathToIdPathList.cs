using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_DirPathToIdPathList
{
	public class RequestData
	{
		[JsonProperty("path")]
		public string? Path { get; set; }

		[JsonProperty("topDirectoryOnly")]
		public bool TopDirectoryOnly { get; set; }
	}

	public class IdPathItem
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("path")]
		public string Path { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("items")]
		public List<IdPathItem> Items { get; set; }
	}
}
