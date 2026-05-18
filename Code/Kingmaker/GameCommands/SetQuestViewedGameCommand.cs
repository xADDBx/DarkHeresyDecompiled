using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SetQuestViewedGameCommand : GameCommand, IMemoryPackable<SetQuestViewedGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetQuestViewedGameCommand>
{
	[Preserve]
	private sealed class SetQuestViewedGameCommandFormatter : MemoryPackFormatter<SetQuestViewedGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetQuestViewedGameCommand value)
		{
			SetQuestViewedGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetQuestViewedGameCommand value)
		{
			SetQuestViewedGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetQuestViewedGameCommand value)
		{
			SetQuestViewedGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetQuestViewedGameCommand value)
		{
			SetQuestViewedGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityFactRef<Quest> m_QuestRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetQuestViewedGameCommand()
	{
	}

	[JsonConstructor]
	public SetQuestViewedGameCommand(Quest quest)
	{
		m_QuestRef = quest;
	}

	protected override void ExecuteInternal()
	{
		Quest fact = m_QuestRef.Fact;
		if (fact != null)
		{
			fact.IsViewed = true;
		}
	}

	static SetQuestViewedGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetQuestViewedGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_QuestRef", typeof(EntityFactRef<Quest>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestViewedGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetQuestViewedGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestViewedGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetQuestViewedGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetQuestViewedGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_QuestRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetQuestViewedGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityFactRef<Quest> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_QuestRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityFactRef<Quest>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetQuestViewedGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_QuestRef : default(EntityFactRef<Quest>));
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
		value = new SetQuestViewedGameCommand
		{
			m_QuestRef = value2
		};
		return;
		IL_0070:
		value.m_QuestRef = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetQuestViewedGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_QuestRef");
		writer.WritePackable(value.m_QuestRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetQuestViewedGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityFactRef<Quest> val = ((value != null) ? value.m_QuestRef : default(EntityFactRef<Quest>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_QuestRef")
				{
					val = reader.ReadPackable<EntityFactRef<Quest>>();
					array[0] = true;
				}
			}
			else if (text == "m_QuestRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_QuestRef = val;
		}
		else
		{
			value = new SetQuestViewedGameCommand
			{
				m_QuestRef = val
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
		SetQuestViewedGameCommand source = new SetQuestViewedGameCommand();
		result = Unsafe.As<SetQuestViewedGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetQuestViewedGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_QuestRef", ref m_QuestRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetQuestViewedGameCommand>();
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
				m_QuestRef = formatter.ReadPackable<EntityFactRef<Quest>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
