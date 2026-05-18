using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class ProcessingBlueprint_IdToContainsShadowDeleted
{
	public class RequestData
	{
		[JsonProperty("id")]
		public string? Id { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty("contains_shadow_deleted_blueprints")]
		public bool? ContainsShadowDeletedBlueprints { get; set; }
	}
}
