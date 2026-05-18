using System;
using System.Buffers;
using Kingmaker.Blueprints.Cargo;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintCargoReference : BlueprintReference<BlueprintCargo>, IMemoryPackable<BlueprintCargoReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintCargoReferenceFormatter : MemoryPackFormatter<BlueprintCargoReference>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintCargoReference value)
		{
			BlueprintCargoReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintCargoReference value)
		{
			BlueprintCargoReference.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintCargoReference value)
		{
			BlueprintCargoReference.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintCargoReference value)
		{
			BlueprintCargoReference.DeserializeJson(ref reader, ref value);
		}
	}

	static BlueprintCargoReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCargoReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintCargoReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCargoReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintCargoReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintCargoReference? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.guid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintCargoReference? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string text;
		if (memberCount == 1)
		{
			if (value != null)
			{
				text = value.guid;
				text = reader.ReadString();
				goto IL_0068;
			}
			text = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintCargoReference), 1, memberCount);
				return;
			}
			text = ((value != null) ? value.guid : null);
			if (memberCount != 0)
			{
				text = reader.ReadString();
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0068;
			}
		}
		value = new BlueprintCargoReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintCargoReference? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("guid");
		writer.WriteString(value.guid);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintCargoReference? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string text = ((value != null) ? value.guid : null);
		bool[] array = new bool[1];
		string text2 = null;
		while ((text2 = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text2 == "guid")
				{
					text = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text2 == "guid")
			{
				text = reader.ReadString();
			}
		}
		if (value != null)
		{
			value.guid = text;
		}
		else
		{
			value = new BlueprintCargoReference
			{
				guid = text
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}
}
