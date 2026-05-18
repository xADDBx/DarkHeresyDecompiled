using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_SearchByType
{
	public class RequestData
	{
		[JsonProperty("type_id_list")]
		public List<string> TypeIdList { get; set; } = new List<string>();

	}

	public class ResponseData
	{
		[JsonProperty("entry_data_list")]
		public List<BlueprintEntryData> EntryDataList { get; set; } = new List<BlueprintEntryData>();

	}
}
