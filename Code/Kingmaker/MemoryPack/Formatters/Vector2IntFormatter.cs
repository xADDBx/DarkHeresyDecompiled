using System;
using JetBrains.Annotations;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class Vector2IntFormatter : MemoryPackFormatter<Vector2Int>
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref Vector2Int value)
	{
		int value2 = value.x;
		writer.WriteUnmanaged(in value2);
		value2 = value.y;
		writer.WriteUnmanaged(in value2);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref Vector2Int value)
	{
		int x = reader.ReadUnmanaged<int>();
		int y = reader.ReadUnmanaged<int>();
		value = new Vector2Int(x, y);
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref Vector2Int value)
	{
		writer.WriteArrayHeader();
		writer.WriteUnmanaged(value.x);
		writer.WriteUnmanaged(value.y);
		writer.WriteArrayFooter();
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref Vector2Int value)
	{
		if (!reader.CheckArrayStart())
		{
			throw new Exception("Expected array start");
		}
		reader.Advance();
		reader.ReadUnmanaged<int>(out var v);
		reader.ReadUnmanaged<int>(out var v2);
		value = new Vector2Int(v, v2);
		if (!reader.CheckArrayEnd())
		{
			throw new Exception("Expected array end");
		}
		reader.Advance();
	}
}
