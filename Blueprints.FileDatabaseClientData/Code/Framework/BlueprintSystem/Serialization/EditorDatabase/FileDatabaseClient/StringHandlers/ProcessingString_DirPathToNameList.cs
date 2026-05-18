using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_DirPathToNameList
{
	public class RequestData
	{
		[JsonProperty("path")]
		public string? Path { get; set; }

		[JsonProperty("topDirectoryOnly")]
		public bool TopDirectoryOnly { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("nameList")]
		public List<string> NameList { get; set; }
	}
}
