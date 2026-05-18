using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class RemoveFromBuyVendorGameCommand : GameCommand, IMemoryPackable<RemoveFromBuyVendorGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<RemoveFromBuyVendorGameCommand>
{
	[Preserve]
	private sealed class RemoveFromBuyVendorGameCommandFormatter : MemoryPackFormatter<RemoveFromBuyVendorGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RemoveFromBuyVendorGameCommand value)
		{
			RemoveFromBuyVendorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref RemoveFromBuyVendorGameCommand value)
		{
			RemoveFromBuyVendorGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RemoveFromBuyVendorGameCommand value)
		{
			RemoveFromBuyVendorGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref RemoveFromBuyVendorGameCommand value)
		{
			RemoveFromBuyVendorGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RemoveFromBuyVendorGameCommand()
	{
	}

	[JsonConstructor]
	public RemoveFromBuyVendorGameCommand(ItemEntity itemEntity, int count)
	{
		m_Item = itemEntity;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.RemoveFromBuy(m_Item, m_Count);
	}

	static RemoveFromBuyVendorGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "RemoveFromBuyVendorGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
				new FieldInfo("m_Count", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveFromBuyVendorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RemoveFromBuyVendorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveFromBuyVendorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RemoveFromBuyVendorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RemoveFromBuyVendorGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RemoveFromBuyVendorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		int value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				goto IL_00a1;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<int>(out value3);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RemoveFromBuyVendorGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = 0;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a1;
			}
		}
		value = new RemoveFromBuyVendorGameCommand
		{
			m_Item = value2,
			m_Count = value3
		};
		return;
		IL_00a1:
		value.m_Item = value2;
		value.m_Count = value3;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref RemoveFromBuyVendorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Item");
		writer.WritePackable(value.m_Item);
		writer.WriteProperty("m_Count");
		writer.WriteUnmanaged(value.m_Count);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref RemoveFromBuyVendorGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<ItemEntity> val;
		int v;
		if (value == null)
		{
			val = default(EntityRef<ItemEntity>);
			v = 0;
		}
		else
		{
			val = value.m_Item;
			v = value.m_Count;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Item"))
				{
					if (text == "m_Count")
					{
						reader.ReadUnmanaged<int>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[0] = true;
				}
			}
			else if (!(text == "m_Item"))
			{
				if (text == "m_Count")
				{
					reader.ReadUnmanaged<int>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_Item = val;
			value.m_Count = v;
		}
		else
		{
			value = new RemoveFromBuyVendorGameCommand
			{
				m_Item = val,
				m_Count = v
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
		RemoveFromBuyVendorGameCommand source = new RemoveFromBuyVendorGameCommand();
		result = Unsafe.As<RemoveFromBuyVendorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RemoveFromBuyVendorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(1, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RemoveFromBuyVendorGameCommand>();
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
				m_Item = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 1:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
