using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_GetAllAssetLinks
{
	public class ResponseData
	{
		[JsonProperty("asset_list")]
		public Dictionary<string, HashSet<string>> AssetList { get; set; } = new Dictionary<string, HashSet<string>>();

	}
}
