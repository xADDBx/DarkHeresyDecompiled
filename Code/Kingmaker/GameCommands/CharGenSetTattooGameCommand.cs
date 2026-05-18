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
public sealed class CharGenSetTattooGameCommand : GameCommand, IMemoryPackable<CharGenSetTattooGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetTattooGameCommand>
{
	[Preserve]
	private sealed class CharGenSetTattooGameCommandFormatter : MemoryPackFormatter<CharGenSetTattooGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetTattooGameCommand value)
		{
			CharGenSetTattooGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetTattooGameCommand value)
		{
			CharGenSetTattooGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetTattooGameCommand value)
		{
			CharGenSetTattooGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetTattooGameCommand value)
		{
			CharGenSetTattooGameCommand.DeserializeJson(ref reader, ref value);
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
	private readonly int m_TattooTabIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetTattooGameCommand([NotNull] EquipmentEntityLink m_equipmentEntityLink, int m_index, int m_tattooTabIndex)
	{
		if (m_equipmentEntityLink == null)
		{
			throw new ArgumentNullException("m_equipmentEntityLink");
		}
		m_EquipmentEntityLink = m_equipmentEntityLink;
		m_Index = m_index;
		m_TattooTabIndex = m_tattooTabIndex;
	}

	private CharGenSetTattooGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetTattoo(m_EquipmentEntityLink, m_Index, m_TattooTabIndex);
		});
	}

	static CharGenSetTattooGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetTattooGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_EquipmentEntityLink", typeof(EquipmentEntityLink)),
				new FieldInfo("m_Index", typeof(int)),
				new FieldInfo("m_TattooTabIndex", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetTattooGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetTattooGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetTattooGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetTattooGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetTattooGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_EquipmentEntityLink);
		writer.WriteUnmanaged(in value.m_Index, in value.m_TattooTabIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetTattooGameCommand? value)
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
				value4 = value.m_TattooTabIndex;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetTattooGameCommand), 3, memberCount);
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
				value4 = value.m_TattooTabIndex;
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
		value = new CharGenSetTattooGameCommand(value2, value3, value4);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetTattooGameCommand? value)
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
		writer.WriteProperty("m_TattooTabIndex");
		writer.WriteUnmanaged(value.m_TattooTabIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetTattooGameCommand? value)
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
			v2 = value.m_TattooTabIndex;
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
				case "m_TattooTabIndex":
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
				case "m_TattooTabIndex":
					reader.ReadUnmanaged<int>(out v2);
					break;
				}
			}
		}
		_ = value;
		value = new CharGenSetTattooGameCommand(val, v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetTattooGameCommand source = new CharGenSetTattooGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetTattooGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetTattooGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EquipmentEntityLink value = m_EquipmentEntityLink;
		formatter.Field(0, "m_EquipmentEntityLink", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		int value3 = m_TattooTabIndex;
		formatter.UnmanagedField(2, "m_TattooTabIndex", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetTattooGameCommand>();
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
				Unsafe.AsRef(in m_TattooTabIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
