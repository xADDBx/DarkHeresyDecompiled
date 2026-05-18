using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.StringHandlers;

public class ProcessingString_ListOfDuplicates
{
	public class ResponseData
	{
		[JsonProperty("duplicated_Id_list")]
		public List<string> DuplicatedIdList { get; set; }
	}
}
