using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class StopUnitsGameCommand : GameCommand, IMemoryPackable<StopUnitsGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<StopUnitsGameCommand>
{
	[Preserve]
	private sealed class StopUnitsGameCommandFormatter : MemoryPackFormatter<StopUnitsGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StopUnitsGameCommand value)
		{
			StopUnitsGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref StopUnitsGameCommand value)
		{
			StopUnitsGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StopUnitsGameCommand value)
		{
			StopUnitsGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref StopUnitsGameCommand value)
		{
			StopUnitsGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity>[] m_Units;

	private static readonly Func<AbstractUnitCommand, bool> NotStarted;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[MemoryPackConstructor]
	private StopUnitsGameCommand(EntityRef<BaseUnitEntity>[] m_units)
	{
		m_Units = m_units;
	}

	private StopUnitsGameCommand(OwlPackConstructorParameter _)
	{
	}

	public StopUnitsGameCommand(IList<BaseUnitEntity> units)
	{
		int count = units.Count;
		m_Units = new EntityRef<BaseUnitEntity>[count];
		for (int i = 0; i < count; i++)
		{
			m_Units[i] = units[i];
		}
	}

	protected override void ExecuteInternal()
	{
		EntityRef<BaseUnitEntity>[] units = m_Units;
		foreach (BaseUnitEntity baseUnitEntity in units)
		{
			if (baseUnitEntity != null)
			{
				baseUnitEntity.HoldState = false;
				baseUnitEntity.Commands.InterruptAll(NotStarted);
				baseUnitEntity.CombatState.LastTarget = null;
				baseUnitEntity.CombatState.ManualTarget = null;
			}
		}
	}

	static StopUnitsGameCommand()
	{
		NotStarted = (AbstractUnitCommand unitCommand) => !unitCommand.IsStarted;
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "StopUnitsGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Units", typeof(EntityRef<BaseUnitEntity>[]))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StopUnitsGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StopUnitsGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StopUnitsGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StopUnitsGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef<BaseUnitEntity>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityRef<BaseUnitEntity>>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StopUnitsGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackableArray(value.m_Units);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StopUnitsGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity>[] value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackableArray<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_Units;
				reader.ReadPackableArray(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StopUnitsGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Units : null);
			if (memberCount != 0)
			{
				reader.ReadPackableArray(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new StopUnitsGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref StopUnitsGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Units");
		writer.WritePackableArray(value.m_Units);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref StopUnitsGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<BaseUnitEntity>[] value2 = ((value != null) ? value.m_Units : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Units")
				{
					value2 = reader.ReadPackableArray<EntityRef<BaseUnitEntity>>();
					array[0] = true;
				}
			}
			else if (text == "m_Units")
			{
				reader.ReadPackableArray(ref value2);
			}
		}
		_ = value;
		value = new StopUnitsGameCommand(value2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StopUnitsGameCommand source = new StopUnitsGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StopUnitsGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StopUnitsGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity>[] value = m_Units;
		formatter.Field(0, "m_Units", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StopUnitsGameCommand>();
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
				Unsafe.AsRef(in m_Units) = formatter.ReadPackable<EntityRef<BaseUnitEntity>[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
