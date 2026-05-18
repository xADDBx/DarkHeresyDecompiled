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
public class SplitSlotGameCommand : GameCommand, IMemoryPackable<SplitSlotGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SplitSlotGameCommand>
{
	[Preserve]
	private sealed class SplitSlotGameCommandFormatter : MemoryPackFormatter<SplitSlotGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SplitSlotGameCommand value)
		{
			SplitSlotGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SplitSlotGameCommand value)
		{
			SplitSlotGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SplitSlotGameCommand value)
		{
			SplitSlotGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SplitSlotGameCommand value)
		{
			SplitSlotGameCommand.DeserializeJson(ref reader, ref value);
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

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_IsLoot;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SplitSlotGameCommand()
	{
	}

	[JsonConstructor]
	public SplitSlotGameCommand(ItemSlotRef from, ItemSlotRef to, bool isLoot, int count)
	{
		m_From = from;
		m_To = to;
		m_IsLoot = isLoot;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TrySplitSlot(m_From, m_To, m_IsLoot, m_Count);
	}

	static SplitSlotGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SplitSlotGameCommand",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("m_From", typeof(ItemSlotRef)),
				new FieldInfo("m_To", typeof(ItemSlotRef)),
				new FieldInfo("m_IsLoot", typeof(bool)),
				new FieldInfo("m_Count", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SplitSlotGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SplitSlotGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SplitSlotGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SplitSlotGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SplitSlotGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
		writer.WriteUnmanaged(in value.m_IsLoot, in value.m_Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SplitSlotGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		bool value4;
		int value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_IsLoot;
				value5 = value.m_Count;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<int>(out value5);
				goto IL_00f3;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
			reader.ReadUnmanaged<bool, int>(out value4, out value5);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SplitSlotGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = false;
				value5 = 0;
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_IsLoot;
				value5 = value.m_Count;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<int>(out value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f3;
			}
		}
		value = new SplitSlotGameCommand
		{
			m_From = value2,
			m_To = value3,
			m_IsLoot = value4,
			m_Count = value5
		};
		return;
		IL_00f3:
		value.m_From = value2;
		value.m_To = value3;
		value.m_IsLoot = value4;
		value.m_Count = value5;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SplitSlotGameCommand? value)
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
		writer.WriteProperty("m_IsLoot");
		writer.WriteUnmanaged(value.m_IsLoot);
		writer.WriteProperty("m_Count");
		writer.WriteUnmanaged(value.m_Count);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SplitSlotGameCommand? value)
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
		bool v;
		int v2;
		if (value == null)
		{
			val = null;
			val2 = null;
			v = false;
			v2 = 0;
		}
		else
		{
			val = value.m_From;
			val2 = value.m_To;
			v = value.m_IsLoot;
			v2 = value.m_Count;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_From":
					val = reader.ReadPackable<ItemSlotRef>();
					array[0] = true;
					break;
				case "m_To":
					val2 = reader.ReadPackable<ItemSlotRef>();
					array[1] = true;
					break;
				case "m_IsLoot":
					reader.ReadUnmanaged<bool>(out v);
					array[2] = true;
					break;
				case "m_Count":
					reader.ReadUnmanaged<int>(out v2);
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_From":
					reader.ReadPackable(ref val);
					break;
				case "m_To":
					reader.ReadPackable(ref val2);
					break;
				case "m_IsLoot":
					reader.ReadUnmanaged<bool>(out v);
					break;
				case "m_Count":
					reader.ReadUnmanaged<int>(out v2);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_From = val;
			value.m_To = val2;
			value.m_IsLoot = v;
			value.m_Count = v2;
		}
		else
		{
			value = new SplitSlotGameCommand
			{
				m_From = val,
				m_To = val2,
				m_IsLoot = v,
				m_Count = v2
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
		SplitSlotGameCommand source = new SplitSlotGameCommand();
		result = Unsafe.As<SplitSlotGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SplitSlotGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.UnmanagedField(2, "m_IsLoot", ref m_IsLoot, state);
		formatter.UnmanagedField(3, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SplitSlotGameCommand>();
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
			case 2:
				m_IsLoot = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
