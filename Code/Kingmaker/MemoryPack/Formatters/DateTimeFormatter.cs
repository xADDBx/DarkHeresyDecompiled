using System;
using JetBrains.Annotations;
using MemoryPack;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class DateTimeFormatter : MemoryPackFormatter<DateTime>
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref DateTime value)
	{
		long value2 = value.ToBinary();
		writer.WriteUnmanaged(in value2);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref DateTime value)
	{
		long dateData = reader.ReadUnmanaged<long>();
		value = DateTime.FromBinary(dateData);
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref DateTime value)
	{
		writer.WriteUnmanaged(value.ToBinary());
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref DateTime value)
	{
		long v = 0L;
		reader.ReadUnmanaged<long>(out v);
		value = DateTime.FromBinary(v);
	}
}
