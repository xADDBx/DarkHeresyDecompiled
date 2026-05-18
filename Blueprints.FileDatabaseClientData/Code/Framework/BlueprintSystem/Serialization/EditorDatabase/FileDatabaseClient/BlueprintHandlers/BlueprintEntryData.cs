using Newtonsoft.Json;

namespace Code.Framework.BlueprintSystem.Serialization.EditorDatabase.FileDatabaseClient.BlueprintHandlers;

public class BlueprintEntryData
{
	[JsonProperty("id")]
	public string? Id { get; set; }

	[JsonProperty("path")]
	public string? Path { get; set; }

	[JsonProperty("is_shadow_deleted")]
	public bool? IsShadowDeleted { get; set; }

	[JsonProperty("contains_shadow_deleted_blueprints")]
	public bool? ContainsShadowDeletedBlueprints { get; set; }

	[JsonProperty("has_obsolete_components")]
	public bool? HasObsoleteComponents { get; set; }
}
