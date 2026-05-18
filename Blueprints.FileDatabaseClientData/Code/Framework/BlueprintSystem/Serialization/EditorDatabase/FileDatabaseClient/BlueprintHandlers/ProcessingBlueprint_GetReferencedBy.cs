using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_GetReferencedBy
{
	public class RequestData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("entry_data_list")]
		public List<BlueprintEntryData> EntryDataList { get; set; }
	}
}
