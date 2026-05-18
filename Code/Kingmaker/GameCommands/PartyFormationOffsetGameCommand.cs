using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PartyFormationOffsetGameCommand : GameCommand, IMemoryPackable<PartyFormationOffsetGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PartyFormationOffsetGameCommand>
{
	[Preserve]
	private sealed class PartyFormationOffsetGameCommandFormatter : MemoryPackFormatter<PartyFormationOffsetGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PartyFormationOffsetGameCommand value)
		{
			PartyFormationOffsetGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PartyFormationOffsetGameCommand value)
		{
			PartyFormationOffsetGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PartyFormationOffsetGameCommand value)
		{
			PartyFormationOffsetGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PartyFormationOffsetGameCommand value)
		{
			PartyFormationOffsetGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_FormationIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_Index;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly UnitReference m_Unit;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly Vector2 m_Vector;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private PartyFormationOffsetGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PartyFormationOffsetGameCommand(int m_formationIndex, int m_index, BaseUnitEntity m_unit, Vector2 m_vector)
	{
		m_FormationIndex = m_formationIndex;
		m_Index = m_index;
		m_Unit = m_unit.FromBaseUnitEntity();
		m_Vector = m_vector;
	}

	public PartyFormationOffsetGameCommand(int m_formationIndex, int m_index, UnitReference m_unit, Vector2 m_vector)
	{
		m_FormationIndex = m_formationIndex;
		m_Index = m_index;
		m_Unit = m_unit;
		m_Vector = m_vector;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.SetOffset(m_FormationIndex, m_Index, m_Unit.ToBaseUnitEntity(), m_Vector);
	}

	static PartyFormationOffsetGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PartyFormationOffsetGameCommand",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("m_FormationIndex", typeof(int)),
				new FieldInfo("m_Index", typeof(int)),
				new FieldInfo("m_Unit", typeof(UnitReference)),
				new FieldInfo("m_Vector", typeof(Vector2))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationOffsetGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PartyFormationOffsetGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationOffsetGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PartyFormationOffsetGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PartyFormationOffsetGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(4, in value.m_FormationIndex, in value.m_Index);
		writer.WritePackable(in value.m_Unit);
		writer.WriteUnmanaged(in value.m_Vector);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PartyFormationOffsetGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		int value3;
		UnitReference value4;
		Vector2 value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, int>(out value2, out value3);
				value4 = reader.ReadPackable<UnitReference>();
				reader.ReadUnmanaged<Vector2>(out value5);
			}
			else
			{
				value2 = value.m_FormationIndex;
				value3 = value.m_Index;
				value4 = value.m_Unit;
				value5 = value.m_Vector;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadPackable(ref value4);
				reader.ReadUnmanaged<Vector2>(out value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PartyFormationOffsetGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
				value4 = default(UnitReference);
				value5 = default(Vector2);
			}
			else
			{
				value2 = value.m_FormationIndex;
				value3 = value.m_Index;
				value4 = value.m_Unit;
				value5 = value.m_Vector;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<Vector2>(out value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new PartyFormationOffsetGameCommand(value2, value3, value4, value5);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PartyFormationOffsetGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_FormationIndex");
		writer.WriteUnmanaged(value.m_FormationIndex);
		writer.WriteProperty("m_Index");
		writer.WriteUnmanaged(value.m_Index);
		writer.WriteProperty("m_Unit");
		writer.WritePackable(value.m_Unit);
		writer.WriteProperty("m_Vector");
		writer.WriteUnmanaged(value.m_Vector);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PartyFormationOffsetGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v;
		int v2;
		UnitReference val;
		Vector2 v3;
		if (value == null)
		{
			v = 0;
			v2 = 0;
			val = default(UnitReference);
			v3 = default(Vector2);
		}
		else
		{
			v = value.m_FormationIndex;
			v2 = value.m_Index;
			val = value.m_Unit;
			v3 = value.m_Vector;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_FormationIndex":
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
					break;
				case "m_Index":
					reader.ReadUnmanaged<int>(out v2);
					array[1] = true;
					break;
				case "m_Unit":
					val = reader.ReadPackable<UnitReference>();
					array[2] = true;
					break;
				case "m_Vector":
					reader.ReadUnmanaged<Vector2>(out v3);
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_FormationIndex":
					reader.ReadUnmanaged<int>(out v);
					break;
				case "m_Index":
					reader.ReadUnmanaged<int>(out v2);
					break;
				case "m_Unit":
					reader.ReadPackable(ref val);
					break;
				case "m_Vector":
					reader.ReadUnmanaged<Vector2>(out v3);
					break;
				}
			}
		}
		_ = value;
		value = new PartyFormationOffsetGameCommand(v, v2, val, v3);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartyFormationOffsetGameCommand source = new PartyFormationOffsetGameCommand();
		result = Unsafe.As<PartyFormationOffsetGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartyFormationOffsetGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_FormationIndex;
		formatter.UnmanagedField(0, "m_FormationIndex", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		UnitReference value3 = m_Unit;
		formatter.Field(2, "m_Unit", ref value3, state);
		Vector2 value4 = m_Vector;
		formatter.Field(3, "m_Vector", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyFormationOffsetGameCommand>();
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
				Unsafe.AsRef(in m_FormationIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_Unit) = formatter.ReadPackable<UnitReference>(state);
				break;
			case 3:
				Unsafe.AsRef(in m_Vector) = formatter.ReadPackable<Vector2>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
