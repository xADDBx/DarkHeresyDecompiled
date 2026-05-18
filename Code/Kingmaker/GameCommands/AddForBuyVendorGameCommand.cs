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
public class AddForBuyVendorGameCommand : GameCommand, IMemoryPackable<AddForBuyVendorGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<AddForBuyVendorGameCommand>
{
	[Preserve]
	private sealed class AddForBuyVendorGameCommandFormatter : MemoryPackFormatter<AddForBuyVendorGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AddForBuyVendorGameCommand value)
		{
			AddForBuyVendorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AddForBuyVendorGameCommand value)
		{
			AddForBuyVendorGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AddForBuyVendorGameCommand value)
		{
			AddForBuyVendorGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AddForBuyVendorGameCommand value)
		{
			AddForBuyVendorGameCommand.DeserializeJson(ref reader, ref value);
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

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_MakeDeal;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AddForBuyVendorGameCommand()
	{
	}

	[JsonConstructor]
	public AddForBuyVendorGameCommand(ItemEntity itemEntity, int count, bool makeDeal)
	{
		m_Item = itemEntity;
		m_Count = count;
		m_MakeDeal = makeDeal;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.AddForBuy(m_Item, m_Count);
		if (m_MakeDeal)
		{
			Game.Instance.TradeLogic.MakeDealWithCurrentVendor();
		}
	}

	static AddForBuyVendorGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "AddForBuyVendorGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
				new FieldInfo("m_Count", typeof(int)),
				new FieldInfo("m_MakeDeal", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AddForBuyVendorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AddForBuyVendorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AddForBuyVendorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AddForBuyVendorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AddForBuyVendorGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Count, in value.m_MakeDeal);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AddForBuyVendorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		int value3;
		bool value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
				value4 = value.m_MakeDeal;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				goto IL_00c9;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<int, bool>(out value3, out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AddForBuyVendorGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = 0;
				value4 = false;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
				value4 = value.m_MakeDeal;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c9;
			}
		}
		value = new AddForBuyVendorGameCommand
		{
			m_Item = value2,
			m_Count = value3,
			m_MakeDeal = value4
		};
		return;
		IL_00c9:
		value.m_Item = value2;
		value.m_Count = value3;
		value.m_MakeDeal = value4;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AddForBuyVendorGameCommand? value)
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
		writer.WriteProperty("m_MakeDeal");
		writer.WriteUnmanaged(value.m_MakeDeal);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AddForBuyVendorGameCommand? value)
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
		bool v2;
		if (value == null)
		{
			val = default(EntityRef<ItemEntity>);
			v = 0;
			v2 = false;
		}
		else
		{
			val = value.m_Item;
			v = value.m_Count;
			v2 = value.m_MakeDeal;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_Item":
					val = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[0] = true;
					break;
				case "m_Count":
					reader.ReadUnmanaged<int>(out v);
					array[1] = true;
					break;
				case "m_MakeDeal":
					reader.ReadUnmanaged<bool>(out v2);
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_Item":
					reader.ReadPackable(ref val);
					break;
				case "m_Count":
					reader.ReadUnmanaged<int>(out v);
					break;
				case "m_MakeDeal":
					reader.ReadUnmanaged<bool>(out v2);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_Item = val;
			value.m_Count = v;
			value.m_MakeDeal = v2;
		}
		else
		{
			value = new AddForBuyVendorGameCommand
			{
				m_Item = val,
				m_Count = v,
				m_MakeDeal = v2
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
		AddForBuyVendorGameCommand source = new AddForBuyVendorGameCommand();
		result = Unsafe.As<AddForBuyVendorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AddForBuyVendorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(1, "m_Count", ref m_Count, state);
		formatter.UnmanagedField(2, "m_MakeDeal", ref m_MakeDeal, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AddForBuyVendorGameCommand>();
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
			case 2:
				m_MakeDeal = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
