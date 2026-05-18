using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_SearchByName
{
	public class RequestData
	{
		[JsonProperty("name_list")]
		public List<string> NameList { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("entry_data_list")]
		public List<PayloadEntryData> EntryDataList { get; set; } = new List<PayloadEntryData>();

	}

	public class PayloadEntryData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }

		[JsonProperty("path")]
		public string? Path { get; set; }
	}
}
