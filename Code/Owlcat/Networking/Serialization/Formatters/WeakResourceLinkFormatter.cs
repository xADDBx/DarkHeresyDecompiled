using JetBrains.Annotations;
using Kingmaker.ResourceLinks;
using MemoryPack;

namespace Owlcat.Networking.Serialization.Formatters;

public sealed class WeakResourceLinkFormatter<TLink> : MemoryPackFormatter<TLink> where TLink : WeakResourceLink, new()
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref TLink value)
	{
		writer.WriteString(value?.AssetId);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref TLink value)
	{
		string text = reader.ReadString();
		if (text == null)
		{
			value = null;
			return;
		}
		if ((object)value == null)
		{
			value = new TLink();
		}
		value.AssetId = text;
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TLink value)
	{
		writer.WriteString(value?.AssetId);
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TLink value)
	{
		string text = reader.ReadString();
		if (text == null)
		{
			value = null;
			return;
		}
		if ((object)value == null)
		{
			value = new TLink();
		}
		value.AssetId = text;
	}
}
