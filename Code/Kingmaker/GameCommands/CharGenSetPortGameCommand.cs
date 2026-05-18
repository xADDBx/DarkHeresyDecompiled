using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPortGameCommand : GameCommand, IMemoryPackable<CharGenSetPortGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetPortGameCommand>
{
	[Preserve]
	private sealed class CharGenSetPortGameCommandFormatter : MemoryPackFormatter<CharGenSetPortGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetPortGameCommand value)
		{
			CharGenSetPortGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetPortGameCommand value)
		{
			CharGenSetPortGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortGameCommand value)
		{
			CharGenSetPortGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetPortGameCommand value)
		{
			CharGenSetPortGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EquipmentEntityLink m_EquipmentEntityLink;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_Index;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_PortNumber;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetPortGameCommand([NotNull] EquipmentEntityLink m_equipmentEntityLink, int m_index, int m_portNumber)
	{
		if (m_equipmentEntityLink == null)
		{
			throw new ArgumentNullException("m_equipmentEntityLink");
		}
		m_EquipmentEntityLink = m_equipmentEntityLink;
		m_Index = m_index;
		m_PortNumber = m_portNumber;
	}

	private CharGenSetPortGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetPort(m_EquipmentEntityLink, m_Index, m_PortNumber);
		});
	}

	static CharGenSetPortGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetPortGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_EquipmentEntityLink", typeof(EquipmentEntityLink)),
				new FieldInfo("m_Index", typeof(int)),
				new FieldInfo("m_PortNumber", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPortGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPortGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetPortGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_EquipmentEntityLink);
		writer.WriteUnmanaged(in value.m_Index, in value.m_PortNumber);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EquipmentEntityLink value2;
		int value3;
		int value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EquipmentEntityLink>();
				reader.ReadUnmanaged<int, int>(out value3, out value4);
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
				value4 = value.m_PortNumber;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPortGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
				value4 = 0;
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
				value4 = value.m_PortNumber;
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
		value = new CharGenSetPortGameCommand(value2, value3, value4);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetPortGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_EquipmentEntityLink");
		writer.WritePackable(value.m_EquipmentEntityLink);
		writer.WriteProperty("m_Index");
		writer.WriteUnmanaged(value.m_Index);
		writer.WriteProperty("m_PortNumber");
		writer.WriteUnmanaged(value.m_PortNumber);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetPortGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EquipmentEntityLink val;
		int v;
		int v2;
		if (value == null)
		{
			val = null;
			v = 0;
			v2 = 0;
		}
		else
		{
			val = value.m_EquipmentEntityLink;
			v = value.m_Index;
			v2 = value.m_PortNumber;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_EquipmentEntityLink":
					val = reader.ReadPackable<EquipmentEntityLink>();
					array[0] = true;
					break;
				case "m_Index":
					reader.ReadUnmanaged<int>(out v);
					array[1] = true;
					break;
				case "m_PortNumber":
					reader.ReadUnmanaged<int>(out v2);
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_EquipmentEntityLink":
					reader.ReadPackable(ref val);
					break;
				case "m_Index":
					reader.ReadUnmanaged<int>(out v);
					break;
				case "m_PortNumber":
					reader.ReadUnmanaged<int>(out v2);
					break;
				}
			}
		}
		_ = value;
		value = new CharGenSetPortGameCommand(val, v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetPortGameCommand source = new CharGenSetPortGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetPortGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetPortGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EquipmentEntityLink value = m_EquipmentEntityLink;
		formatter.Field(0, "m_EquipmentEntityLink", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		int value3 = m_PortNumber;
		formatter.UnmanagedField(2, "m_PortNumber", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetPortGameCommand>();
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
				Unsafe.AsRef(in m_EquipmentEntityLink) = formatter.ReadPackable<EquipmentEntityLink>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_PortNumber) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
