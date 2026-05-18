using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_SearchByName
{
	public class RequestData
	{
		[JsonProperty("name_list")]
		public List<string> NameList { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("entry_data_list")]
		public List<BlueprintEntryData> EntryDataList { get; set; } = new List<BlueprintEntryData>();

	}
}
