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
public sealed class MarkHighlightedAndNoticedGameCommand : GameCommand, IMemoryPackable<MarkHighlightedAndNoticedGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<MarkHighlightedAndNoticedGameCommand>
{
	[Preserve]
	private sealed class MarkHighlightedAndNoticedGameCommandFormatter : MemoryPackFormatter<MarkHighlightedAndNoticedGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MarkHighlightedAndNoticedGameCommand value)
		{
			MarkHighlightedAndNoticedGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref MarkHighlightedAndNoticedGameCommand value)
		{
			MarkHighlightedAndNoticedGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref MarkHighlightedAndNoticedGameCommand value)
		{
			MarkHighlightedAndNoticedGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref MarkHighlightedAndNoticedGameCommand value)
		{
			MarkHighlightedAndNoticedGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<MapObjectEntity> m_MapObjectEntityRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private MarkHighlightedAndNoticedGameCommand()
	{
	}

	[JsonConstructor]
	public MarkHighlightedAndNoticedGameCommand(EntityRef<MapObjectEntity> mapObjectEntityRef)
	{
		m_MapObjectEntityRef = mapObjectEntityRef;
	}

	protected override void ExecuteInternal()
	{
		MapObjectEntity entity = m_MapObjectEntityRef.Entity;
		if (entity != null)
		{
			entity.WasHighlightedOnRevealAndNoticed = true;
			entity.View?.MarkHighlightedAndNoticed();
		}
	}

	static MarkHighlightedAndNoticedGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "MarkHighlightedAndNoticedGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_MapObjectEntityRef", typeof(EntityRef<MapObjectEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<MarkHighlightedAndNoticedGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new MarkHighlightedAndNoticedGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<MarkHighlightedAndNoticedGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<MarkHighlightedAndNoticedGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref MarkHighlightedAndNoticedGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_MapObjectEntityRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref MarkHighlightedAndNoticedGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MapObjectEntity> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_MapObjectEntityRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef<MapObjectEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(MarkHighlightedAndNoticedGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_MapObjectEntityRef : default(EntityRef<MapObjectEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new MarkHighlightedAndNoticedGameCommand
		{
			m_MapObjectEntityRef = value2
		};
		return;
		IL_0070:
		value.m_MapObjectEntityRef = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref MarkHighlightedAndNoticedGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_MapObjectEntityRef");
		writer.WritePackable(value.m_MapObjectEntityRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref MarkHighlightedAndNoticedGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<MapObjectEntity> val = ((value != null) ? value.m_MapObjectEntityRef : default(EntityRef<MapObjectEntity>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_MapObjectEntityRef")
				{
					val = reader.ReadPackable<EntityRef<MapObjectEntity>>();
					array[0] = true;
				}
			}
			else if (text == "m_MapObjectEntityRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_MapObjectEntityRef = val;
		}
		else
		{
			value = new MarkHighlightedAndNoticedGameCommand
			{
				m_MapObjectEntityRef = val
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MarkHighlightedAndNoticedGameCommand source = new MarkHighlightedAndNoticedGameCommand();
		result = Unsafe.As<MarkHighlightedAndNoticedGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MarkHighlightedAndNoticedGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_MapObjectEntityRef", ref m_MapObjectEntityRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MarkHighlightedAndNoticedGameCommand>();
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
				m_MapObjectEntityRef = formatter.ReadPackable<EntityRef<MapObjectEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
