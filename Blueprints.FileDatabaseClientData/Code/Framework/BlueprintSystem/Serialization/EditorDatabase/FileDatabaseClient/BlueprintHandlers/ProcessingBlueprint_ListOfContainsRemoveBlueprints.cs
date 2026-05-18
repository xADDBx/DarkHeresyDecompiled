using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_ListOfContainsRemoveBlueprints
{
	public class ResponseData
	{
		[JsonProperty("contains_remove_blueprints_list")]
		public Dictionary<string, HashSet<string>> ContainsRemoveBlueprintsList { get; set; } = new Dictionary<string, HashSet<string>>();

	}
}
