using System;
using JetBrains.Annotations;
using MemoryPack;

namespace Kingmaker.MemoryPack.Formatters;

public sealed class TimeSpanFormatter : MemoryPackFormatter<TimeSpan>
{
	public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, [CanBeNull] ref TimeSpan value)
	{
		long value2 = value.Ticks;
		writer.WriteUnmanaged(in value2);
	}

	public override void Deserialize(ref MemoryPackReader reader, [CanBeNull] ref TimeSpan value)
	{
		long ticks = reader.ReadUnmanaged<long>();
		value = new TimeSpan(ticks);
	}

	public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TimeSpan value)
	{
		writer.WriteUnmanaged(value.Ticks);
	}

	public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TimeSpan value)
	{
		long v = 0L;
		reader.ReadUnmanaged<long>(out v);
		value = new TimeSpan(v);
	}
}
