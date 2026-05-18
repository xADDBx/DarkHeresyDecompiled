using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetEquipmentColorGameCommand : GameCommand, IMemoryPackable<CharGenSetEquipmentColorGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetEquipmentColorGameCommand>
{
	[Preserve]
	private sealed class CharGenSetEquipmentColorGameCommandFormatter : MemoryPackFormatter<CharGenSetEquipmentColorGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetEquipmentColorGameCommand value)
		{
			CharGenSetEquipmentColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetEquipmentColorGameCommand value)
		{
			CharGenSetEquipmentColorGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetEquipmentColorGameCommand value)
		{
			CharGenSetEquipmentColorGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetEquipmentColorGameCommand value)
		{
			CharGenSetEquipmentColorGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_PrimaryIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_SecondaryIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetEquipmentColorGameCommand(int m_primaryIndex, int m_secondaryIndex)
	{
		m_PrimaryIndex = m_primaryIndex;
		m_SecondaryIndex = m_secondaryIndex;
	}

	private CharGenSetEquipmentColorGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetEquipmentColor(m_PrimaryIndex, m_SecondaryIndex);
		});
	}

	static CharGenSetEquipmentColorGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetEquipmentColorGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_PrimaryIndex", typeof(int)),
				new FieldInfo("m_SecondaryIndex", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetEquipmentColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetEquipmentColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetEquipmentColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetEquipmentColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetEquipmentColorGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_PrimaryIndex, in value.m_SecondaryIndex);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetEquipmentColorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, int>(out value2, out value3);
			}
			else
			{
				value2 = value.m_PrimaryIndex;
				value3 = value.m_SecondaryIndex;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetEquipmentColorGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
			}
			else
			{
				value2 = value.m_PrimaryIndex;
				value3 = value.m_SecondaryIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetEquipmentColorGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetEquipmentColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_PrimaryIndex");
		writer.WriteUnmanaged(value.m_PrimaryIndex);
		writer.WriteProperty("m_SecondaryIndex");
		writer.WriteUnmanaged(value.m_SecondaryIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetEquipmentColorGameCommand? value)
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
		if (value == null)
		{
			v = 0;
			v2 = 0;
		}
		else
		{
			v = value.m_PrimaryIndex;
			v2 = value.m_SecondaryIndex;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_PrimaryIndex"))
				{
					if (text == "m_SecondaryIndex")
					{
						reader.ReadUnmanaged<int>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_PrimaryIndex"))
			{
				if (text == "m_SecondaryIndex")
				{
					reader.ReadUnmanaged<int>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		_ = value;
		value = new CharGenSetEquipmentColorGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetEquipmentColorGameCommand source = new CharGenSetEquipmentColorGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetEquipmentColorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetEquipmentColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_PrimaryIndex;
		formatter.UnmanagedField(0, "m_PrimaryIndex", ref value, state);
		int value2 = m_SecondaryIndex;
		formatter.UnmanagedField(1, "m_SecondaryIndex", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetEquipmentColorGameCommand>();
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
				Unsafe.AsRef(in m_PrimaryIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_SecondaryIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
