using System;
using JetBrains.Annotations;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class Vector3Formatter : MemoryPackFormatter<Vector3>
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref Vector3 value)
	{
		writer.WriteUnmanaged(in value.x);
		writer.WriteUnmanaged(in value.y);
		writer.WriteUnmanaged(in value.z);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref Vector3 value)
	{
		float x = reader.ReadUnmanaged<float>();
		float y = reader.ReadUnmanaged<float>();
		float z = reader.ReadUnmanaged<float>();
		value = new Vector3(x, y, z);
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref Vector3 value)
	{
		writer.WriteArrayHeader();
		writer.WriteUnmanaged(value.x);
		writer.WriteUnmanaged(value.y);
		writer.WriteUnmanaged(value.z);
		writer.WriteArrayFooter();
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref Vector3 value)
	{
		if (!reader.CheckArrayStart())
		{
			throw new Exception("Expected array start");
		}
		reader.Advance();
		reader.ReadUnmanaged<float>(out var v);
		reader.ReadUnmanaged<float>(out var v2);
		reader.ReadUnmanaged<float>(out var v3);
		value = new Vector3(v, v2, v3);
		if (!reader.CheckArrayEnd())
		{
			throw new Exception("Expected array end");
		}
		reader.Advance();
	}
}
