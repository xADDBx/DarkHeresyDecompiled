using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Visual.CharacterSystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class SetEquipmentColorGameCommand : GameCommand, IMemoryPackable<SetEquipmentColorGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetEquipmentColorGameCommand>
{
	[Preserve]
	private sealed class SetEquipmentColorGameCommandFormatter : MemoryPackFormatter<SetEquipmentColorGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetEquipmentColorGameCommand value)
		{
			SetEquipmentColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetEquipmentColorGameCommand value)
		{
			SetEquipmentColorGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetEquipmentColorGameCommand value)
		{
			SetEquipmentColorGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetEquipmentColorGameCommand value)
		{
			SetEquipmentColorGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private UnitReference m_UnitRef;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_PrimaryIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_SecondaryIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SetEquipmentColorGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SetEquipmentColorGameCommand(int m_primaryindex, int m_secondaryindex, UnitReference m_unitref)
	{
		m_PrimaryIndex = m_primaryindex;
		m_SecondaryIndex = m_secondaryindex;
		m_UnitRef = m_unitref;
	}

	public SetEquipmentColorGameCommand(RampColorPreset.IndexSet indexSet, BaseUnitEntity unit)
	{
		m_PrimaryIndex = indexSet.PrimaryIndex;
		m_SecondaryIndex = indexSet.SecondaryIndex;
		m_UnitRef = UnitReference.FromIAbstractUnitEntity(unit);
	}

	protected override void ExecuteInternal()
	{
		RampColorPreset.IndexSet indexSet = new RampColorPreset.IndexSet();
		indexSet.PrimaryIndex = m_PrimaryIndex;
		indexSet.SecondaryIndex = m_SecondaryIndex;
		m_UnitRef.Entity.ToBaseUnitEntity().SetUnitEquipmentColorRampIndex(indexSet);
		m_UnitRef.Entity.ToBaseUnitEntity().View?.UpdateEquipmentColor();
	}

	static SetEquipmentColorGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetEquipmentColorGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_UnitRef", typeof(UnitReference)),
				new FieldInfo("m_PrimaryIndex", typeof(int)),
				new FieldInfo("m_SecondaryIndex", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetEquipmentColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetEquipmentColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetEquipmentColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetEquipmentColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetEquipmentColorGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_UnitRef);
		writer.WriteUnmanaged(in value.m_PrimaryIndex, in value.m_SecondaryIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetEquipmentColorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		int value3;
		int value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<UnitReference>();
				reader.ReadUnmanaged<int, int>(out value3, out value4);
			}
			else
			{
				value2 = value.m_UnitRef;
				value3 = value.m_PrimaryIndex;
				value4 = value.m_SecondaryIndex;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetEquipmentColorGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(UnitReference);
				value3 = 0;
				value4 = 0;
			}
			else
			{
				value2 = value.m_UnitRef;
				value3 = value.m_PrimaryIndex;
				value4 = value.m_SecondaryIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new SetEquipmentColorGameCommand(value3, value4, value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetEquipmentColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_UnitRef");
		writer.WritePackable(value.m_UnitRef);
		writer.WriteProperty("m_PrimaryIndex");
		writer.WriteUnmanaged(value.m_PrimaryIndex);
		writer.WriteProperty("m_SecondaryIndex");
		writer.WriteUnmanaged(value.m_SecondaryIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetEquipmentColorGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		UnitReference val;
		int v;
		int v2;
		if (value == null)
		{
			val = default(UnitReference);
			v = 0;
			v2 = 0;
		}
		else
		{
			val = value.m_UnitRef;
			v = value.m_PrimaryIndex;
			v2 = value.m_SecondaryIndex;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_UnitRef":
					val = reader.ReadPackable<UnitReference>();
					array[0] = true;
					break;
				case "m_PrimaryIndex":
					reader.ReadUnmanaged<int>(out v);
					array[1] = true;
					break;
				case "m_SecondaryIndex":
					reader.ReadUnmanaged<int>(out v2);
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_UnitRef":
					reader.ReadPackable(ref val);
					break;
				case "m_PrimaryIndex":
					reader.ReadUnmanaged<int>(out v);
					break;
				case "m_SecondaryIndex":
					reader.ReadUnmanaged<int>(out v2);
					break;
				}
			}
		}
		_ = value;
		value = new SetEquipmentColorGameCommand(v, v2, val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SetEquipmentColorGameCommand source = new SetEquipmentColorGameCommand();
		result = Unsafe.As<SetEquipmentColorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetEquipmentColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.UnmanagedField(1, "m_PrimaryIndex", ref m_PrimaryIndex, state);
		formatter.UnmanagedField(2, "m_SecondaryIndex", ref m_SecondaryIndex, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetEquipmentColorGameCommand>();
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
				m_UnitRef = formatter.ReadPackable<UnitReference>(state);
				break;
			case 1:
				m_PrimaryIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				m_SecondaryIndex = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
