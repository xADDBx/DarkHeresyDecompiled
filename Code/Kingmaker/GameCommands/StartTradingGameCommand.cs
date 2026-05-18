using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.Vendor;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class StartTradingGameCommand : GameCommandWithSynchronized, IMemoryPackable<StartTradingGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<StartTradingGameCommand>
{
	[Preserve]
	private sealed class StartTradingGameCommandFormatter : MemoryPackFormatter<StartTradingGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StartTradingGameCommand value)
		{
			StartTradingGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref StartTradingGameCommand value)
		{
			StartTradingGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StartTradingGameCommand value)
		{
			StartTradingGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref StartTradingGameCommand value)
		{
			StartTradingGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_Vendor;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	private StartTradingGameCommand(EntityRef<MechanicEntity> m_vendor)
	{
		m_Vendor = m_vendor;
	}

	private StartTradingGameCommand(OwlPackConstructorParameter _)
	{
	}

	public StartTradingGameCommand(MechanicEntity vendor, bool isSynchronized)
		: this(vendor)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		VendorHelper.TradeLogic.BeginTrading(m_Vendor.Entity);
	}

	static StartTradingGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "StartTradingGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Vendor", typeof(EntityRef<MechanicEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StartTradingGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StartTradingGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StartTradingGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StartTradingGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StartTradingGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Vendor);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StartTradingGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			}
			else
			{
				value2 = value.m_Vendor;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StartTradingGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Vendor : default(EntityRef<MechanicEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new StartTradingGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref StartTradingGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Vendor");
		writer.WritePackable(value.m_Vendor);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref StartTradingGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<MechanicEntity> val = ((value != null) ? value.m_Vendor : default(EntityRef<MechanicEntity>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Vendor")
				{
					val = reader.ReadPackable<EntityRef<MechanicEntity>>();
					array[0] = true;
				}
			}
			else if (text == "m_Vendor")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new StartTradingGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StartTradingGameCommand source = new StartTradingGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StartTradingGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StartTradingGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Vendor", ref m_Vendor, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StartTradingGameCommand>();
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
				m_Vendor = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
