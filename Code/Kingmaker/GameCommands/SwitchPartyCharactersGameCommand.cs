using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SwitchPartyCharactersGameCommand : GameCommand, IMemoryPackable<SwitchPartyCharactersGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SwitchPartyCharactersGameCommand>
{
	[Preserve]
	private sealed class SwitchPartyCharactersGameCommandFormatter : MemoryPackFormatter<SwitchPartyCharactersGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SwitchPartyCharactersGameCommand value)
		{
			SwitchPartyCharactersGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SwitchPartyCharactersGameCommand value)
		{
			SwitchPartyCharactersGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SwitchPartyCharactersGameCommand value)
		{
			SwitchPartyCharactersGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SwitchPartyCharactersGameCommand value)
		{
			SwitchPartyCharactersGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly sbyte m_Index1;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly sbyte m_Index2;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit1;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit2;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SwitchPartyCharactersGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SwitchPartyCharactersGameCommand(EntityRef<BaseUnitEntity> m_unit1, EntityRef<BaseUnitEntity> m_unit2, sbyte m_index1, sbyte m_index2)
	{
		m_Unit1 = m_unit1;
		m_Unit2 = m_unit2;
		m_Index1 = m_index1;
		m_Index2 = m_index2;
	}

	public SwitchPartyCharactersGameCommand(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		m_Unit1 = unit1;
		m_Unit2 = unit2;
		m_Index1 = (sbyte)Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit1);
		m_Index2 = (sbyte)Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit2);
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity unit1 = m_Unit1.Entity;
		BaseUnitEntity unit2 = m_Unit2.Entity;
		if (unit1 != null && unit2 != null)
		{
			int num = Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit1);
			int num2 = Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit2);
			if (num == m_Index1 && num2 == m_Index2)
			{
				Game.Instance.Controllers.SelectionCharacter.SwitchCharacter(unit1, unit2);
			}
		}
	}

	static SwitchPartyCharactersGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SwitchPartyCharactersGameCommand",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("m_Index1", typeof(sbyte)),
				new FieldInfo("m_Index2", typeof(sbyte)),
				new FieldInfo("m_Unit1", typeof(EntityRef<BaseUnitEntity>)),
				new FieldInfo("m_Unit2", typeof(EntityRef<BaseUnitEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchPartyCharactersGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SwitchPartyCharactersGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchPartyCharactersGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SwitchPartyCharactersGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SwitchPartyCharactersGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(4, in value.m_Index1, in value.m_Index2);
		writer.WritePackable(in value.m_Unit1);
		writer.WritePackable(in value.m_Unit2);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SwitchPartyCharactersGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		sbyte value2;
		sbyte value3;
		EntityRef<BaseUnitEntity> value4;
		EntityRef<BaseUnitEntity> value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<sbyte, sbyte>(out value2, out value3);
				value4 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				value5 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_Index1;
				value3 = value.m_Index2;
				value4 = value.m_Unit1;
				value5 = value.m_Unit2;
				reader.ReadUnmanaged<sbyte>(out value2);
				reader.ReadUnmanaged<sbyte>(out value3);
				reader.ReadPackable(ref value4);
				reader.ReadPackable(ref value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SwitchPartyCharactersGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
				value4 = default(EntityRef<BaseUnitEntity>);
				value5 = default(EntityRef<BaseUnitEntity>);
			}
			else
			{
				value2 = value.m_Index1;
				value3 = value.m_Index2;
				value4 = value.m_Unit1;
				value5 = value.m_Unit2;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<sbyte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<sbyte>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new SwitchPartyCharactersGameCommand(value4, value5, value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SwitchPartyCharactersGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Index1");
		writer.WriteUnmanaged(value.m_Index1);
		writer.WriteProperty("m_Index2");
		writer.WriteUnmanaged(value.m_Index2);
		writer.WriteProperty("m_Unit1");
		writer.WritePackable(value.m_Unit1);
		writer.WriteProperty("m_Unit2");
		writer.WritePackable(value.m_Unit2);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SwitchPartyCharactersGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		sbyte v;
		sbyte v2;
		EntityRef<BaseUnitEntity> val;
		EntityRef<BaseUnitEntity> val2;
		if (value == null)
		{
			v = 0;
			v2 = 0;
			val = default(EntityRef<BaseUnitEntity>);
			val2 = default(EntityRef<BaseUnitEntity>);
		}
		else
		{
			v = value.m_Index1;
			v2 = value.m_Index2;
			val = value.m_Unit1;
			val2 = value.m_Unit2;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_Index1":
					reader.ReadUnmanaged<sbyte>(out v);
					array[0] = true;
					break;
				case "m_Index2":
					reader.ReadUnmanaged<sbyte>(out v2);
					array[1] = true;
					break;
				case "m_Unit1":
					val = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[2] = true;
					break;
				case "m_Unit2":
					val2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_Index1":
					reader.ReadUnmanaged<sbyte>(out v);
					break;
				case "m_Index2":
					reader.ReadUnmanaged<sbyte>(out v2);
					break;
				case "m_Unit1":
					reader.ReadPackable(ref val);
					break;
				case "m_Unit2":
					reader.ReadPackable(ref val2);
					break;
				}
			}
		}
		_ = value;
		value = new SwitchPartyCharactersGameCommand(val, val2, v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SwitchPartyCharactersGameCommand source = new SwitchPartyCharactersGameCommand();
		result = Unsafe.As<SwitchPartyCharactersGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SwitchPartyCharactersGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		sbyte value = m_Index1;
		formatter.UnmanagedField(0, "m_Index1", ref value, state);
		sbyte value2 = m_Index2;
		formatter.UnmanagedField(1, "m_Index2", ref value2, state);
		EntityRef<BaseUnitEntity> value3 = m_Unit1;
		formatter.Field(2, "m_Unit1", ref value3, state);
		EntityRef<BaseUnitEntity> value4 = m_Unit2;
		formatter.Field(3, "m_Unit2", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SwitchPartyCharactersGameCommand>();
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
				Unsafe.AsRef(in m_Index1) = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index2) = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_Unit1) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 3:
				Unsafe.AsRef(in m_Unit2) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
