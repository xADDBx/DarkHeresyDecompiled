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
public sealed class CharGenSetHairGameCommand : GameCommand, IMemoryPackable<CharGenSetHairGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetHairGameCommand>
{
	[Preserve]
	private sealed class CharGenSetHairGameCommandFormatter : MemoryPackFormatter<CharGenSetHairGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetHairGameCommand value)
		{
			CharGenSetHairGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetHairGameCommand value)
		{
			CharGenSetHairGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetHairGameCommand value)
		{
			CharGenSetHairGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetHairGameCommand value)
		{
			CharGenSetHairGameCommand.DeserializeJson(ref reader, ref value);
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

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetHairGameCommand([NotNull] EquipmentEntityLink m_equipmentEntityLink, int m_index)
	{
		if (m_equipmentEntityLink == null)
		{
			throw new ArgumentNullException("m_equipmentEntityLink");
		}
		m_EquipmentEntityLink = m_equipmentEntityLink;
		m_Index = m_index;
	}

	private CharGenSetHairGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetHair(m_EquipmentEntityLink, m_Index);
		});
	}

	static CharGenSetHairGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetHairGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_EquipmentEntityLink", typeof(EquipmentEntityLink)),
				new FieldInfo("m_Index", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetHairGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetHairGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetHairGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetHairGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetHairGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_EquipmentEntityLink);
		writer.WriteUnmanaged(in value.m_Index);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetHairGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EquipmentEntityLink value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EquipmentEntityLink>();
				reader.ReadUnmanaged<int>(out value3);
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetHairGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
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
			_ = value;
		}
		value = new CharGenSetHairGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetHairGameCommand? value)
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
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetHairGameCommand? value)
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
		if (value == null)
		{
			val = null;
			v = 0;
		}
		else
		{
			val = value.m_EquipmentEntityLink;
			v = value.m_Index;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_EquipmentEntityLink"))
				{
					if (text == "m_Index")
					{
						reader.ReadUnmanaged<int>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<EquipmentEntityLink>();
					array[0] = true;
				}
			}
			else if (!(text == "m_EquipmentEntityLink"))
			{
				if (text == "m_Index")
				{
					reader.ReadUnmanaged<int>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharGenSetHairGameCommand(val, v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetHairGameCommand source = new CharGenSetHairGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetHairGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetHairGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EquipmentEntityLink value = m_EquipmentEntityLink;
		formatter.Field(0, "m_EquipmentEntityLink", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetHairGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
