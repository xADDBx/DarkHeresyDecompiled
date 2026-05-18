using System;
using System.Buffers;
using Kingmaker.UnitLogic.Levelup.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[MemoryPackable(GenerateType.Object)]
public class BlueprintRaceVisualPresetReference : BlueprintReference<BlueprintRaceVisualPreset>, IMemoryPackable<BlueprintRaceVisualPresetReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintRaceVisualPresetReferenceFormatter : MemoryPackFormatter<BlueprintRaceVisualPresetReference>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintRaceVisualPresetReference value)
		{
			BlueprintRaceVisualPresetReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintRaceVisualPresetReference value)
		{
			BlueprintRaceVisualPresetReference.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintRaceVisualPresetReference value)
		{
			BlueprintRaceVisualPresetReference.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintRaceVisualPresetReference value)
		{
			BlueprintRaceVisualPresetReference.DeserializeJson(ref reader, ref value);
		}
	}

	static BlueprintRaceVisualPresetReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintRaceVisualPresetReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintRaceVisualPresetReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintRaceVisualPresetReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintRaceVisualPresetReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintRaceVisualPresetReference? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintRaceVisualPresetReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintRaceVisualPresetReference), 1, memberCount);
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
		value = new BlueprintRaceVisualPresetReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintRaceVisualPresetReference? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintRaceVisualPresetReference? value)
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
			value = new BlueprintRaceVisualPresetReference
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
