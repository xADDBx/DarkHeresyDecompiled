using System;
using System.Buffers;
using Kingmaker.Framework.DetectiveSystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[MemoryPackable(GenerateType.Object)]
public class BlueprintConclusionReference : BlueprintReference<BlueprintConclusion>, IMemoryPackable<BlueprintConclusionReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintConclusionReferenceFormatter : MemoryPackFormatter<BlueprintConclusionReference>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintConclusionReference value)
		{
			BlueprintConclusionReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintConclusionReference value)
		{
			BlueprintConclusionReference.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintConclusionReference value)
		{
			BlueprintConclusionReference.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintConclusionReference value)
		{
			BlueprintConclusionReference.DeserializeJson(ref reader, ref value);
		}
	}

	static BlueprintConclusionReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintConclusionReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintConclusionReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintConclusionReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintConclusionReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintConclusionReference? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintConclusionReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintConclusionReference), 1, memberCount);
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
		value = new BlueprintConclusionReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintConclusionReference? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintConclusionReference? value)
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
			value = new BlueprintConclusionReference
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
