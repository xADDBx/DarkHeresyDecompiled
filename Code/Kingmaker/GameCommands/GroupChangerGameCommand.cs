using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class GroupChangerGameCommand : GameCommand, IMemoryPackable<GroupChangerGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<GroupChangerGameCommand>
{
	[Preserve]
	private sealed class GroupChangerGameCommandFormatter : MemoryPackFormatter<GroupChangerGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref GroupChangerGameCommand value)
		{
			GroupChangerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref GroupChangerGameCommand value)
		{
			GroupChangerGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GroupChangerGameCommand value)
		{
			GroupChangerGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref GroupChangerGameCommand value)
		{
			GroupChangerGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly UnitReference m_UnitReference;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private GroupChangerGameCommand()
	{
	}

	[MemoryPackConstructor]
	public GroupChangerGameCommand(UnitReference m_unitReference)
	{
		m_UnitReference = m_unitReference;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IChangeGroupHandler h)
		{
			h.HandleChangeGroup(m_UnitReference.Id);
		});
	}

	static GroupChangerGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "GroupChangerGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_UnitReference", typeof(UnitReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GroupChangerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new GroupChangerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GroupChangerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<GroupChangerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref GroupChangerGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_UnitReference);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GroupChangerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<UnitReference>();
			}
			else
			{
				value2 = value.m_UnitReference;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GroupChangerGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UnitReference : default(UnitReference));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new GroupChangerGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref GroupChangerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_UnitReference");
		writer.WritePackable(value.m_UnitReference);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref GroupChangerGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		UnitReference val = ((value != null) ? value.m_UnitReference : default(UnitReference));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_UnitReference")
				{
					val = reader.ReadPackable<UnitReference>();
					array[0] = true;
				}
			}
			else if (text == "m_UnitReference")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new GroupChangerGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		GroupChangerGameCommand source = new GroupChangerGameCommand();
		result = Unsafe.As<GroupChangerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<GroupChangerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		UnitReference value = m_UnitReference;
		formatter.Field(0, "m_UnitReference", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<GroupChangerGameCommand>();
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
				Unsafe.AsRef(in m_UnitReference) = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
