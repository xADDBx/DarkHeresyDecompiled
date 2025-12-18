using JetBrains.Annotations;
using Kingmaker.Blueprints;
using MemoryPack;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class BlueprintFormatter<TBlueprint> : MemoryPackFormatter<TBlueprint> where TBlueprint : BlueprintScriptableObject
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref TBlueprint value)
	{
		string value2 = value?.AssetGuid;
		writer.WriteString(value2);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref TBlueprint value)
	{
		string assetId = reader.ReadString();
		value = ResourcesLibrary.TryGetBlueprint<TBlueprint>(assetId);
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TBlueprint value)
	{
		writer.WriteString(value?.AssetGuid);
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TBlueprint value)
	{
		string assetId = reader.ReadString();
		value = ResourcesLibrary.TryGetBlueprint<TBlueprint>(assetId);
	}
}
