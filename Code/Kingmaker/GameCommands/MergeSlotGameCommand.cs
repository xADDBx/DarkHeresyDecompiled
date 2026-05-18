using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class MergeSlotGameCommand : GameCommand, IMemoryPackable<MergeSlotGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<MergeSlotGameCommand>
{
	[Preserve]
	private sealed class MergeSlotGameCommandFormatter : MemoryPackFormatter<MergeSlotGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MergeSlotGameCommand value)
		{
			MergeSlotGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref MergeSlotGameCommand value)
		{
			MergeSlotGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref MergeSlotGameCommand value)
		{
			MergeSlotGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref MergeSlotGameCommand value)
		{
			MergeSlotGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private MergeSlotGameCommand()
	{
	}

	[JsonConstructor]
	public MergeSlotGameCommand(ItemSlotRef from, ItemSlotRef to)
	{
		m_From = from;
		m_To = to;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.MergeSlot(m_From, m_To);
	}

	static MergeSlotGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "MergeSlotGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_From", typeof(ItemSlotRef)),
				new FieldInfo("m_To", typeof(ItemSlotRef))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<MergeSlotGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new MergeSlotGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<MergeSlotGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<MergeSlotGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MergeSlotGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref MergeSlotGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(MergeSlotGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new MergeSlotGameCommand
		{
			m_From = value2,
			m_To = value3
		};
		return;
		IL_009a:
		value.m_From = value2;
		value.m_To = value3;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref MergeSlotGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_From");
		writer.WritePackable(value.m_From);
		writer.WriteProperty("m_To");
		writer.WritePackable(value.m_To);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref MergeSlotGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		ItemSlotRef val;
		ItemSlotRef val2;
		if (value == null)
		{
			val = null;
			val2 = null;
		}
		else
		{
			val = value.m_From;
			val2 = value.m_To;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_From"))
				{
					if (text == "m_To")
					{
						val2 = reader.ReadPackable<ItemSlotRef>();
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<ItemSlotRef>();
					array[0] = true;
				}
			}
			else if (!(text == "m_From"))
			{
				if (text == "m_To")
				{
					reader.ReadPackable(ref val2);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_From = val;
			value.m_To = val2;
		}
		else
		{
			value = new MergeSlotGameCommand
			{
				m_From = val,
				m_To = val2
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MergeSlotGameCommand source = new MergeSlotGameCommand();
		result = Unsafe.As<MergeSlotGameCommand, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<MergeSlotGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MergeSlotGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_From = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			case 1:
				m_To = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
