using System;
using JetBrains.Annotations;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class Vector2Formatter : MemoryPackFormatter<Vector2>
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref Vector2 value)
	{
		writer.WriteUnmanaged(in value.x);
		writer.WriteUnmanaged(in value.y);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref Vector2 value)
	{
		float x = reader.ReadUnmanaged<float>();
		float y = reader.ReadUnmanaged<float>();
		value = new Vector2(x, y);
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref Vector2 value)
	{
		writer.WriteArrayHeader();
		writer.WriteUnmanaged(value.x);
		writer.WriteUnmanaged(value.y);
		writer.WriteArrayFooter();
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref Vector2 value)
	{
		if (!reader.CheckArrayStart())
		{
			throw new Exception("Expected array start");
		}
		reader.Advance();
		reader.ReadUnmanaged<float>(out var v);
		reader.ReadUnmanaged<float>(out var v2);
		value = new Vector2(v, v2);
		if (!reader.CheckArrayEnd())
		{
			throw new Exception("Expected array end");
		}
		reader.Advance();
	}
}
