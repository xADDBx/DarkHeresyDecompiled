using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_IdToIsShadowDeleted
{
	public class RequestData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("is_shadow_deleted")]
		public bool? IsShadowDeleted { get; set; }
	}
}
