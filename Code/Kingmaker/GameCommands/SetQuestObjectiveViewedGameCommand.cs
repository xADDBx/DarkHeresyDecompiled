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
public class SetQuestObjectiveViewedGameCommand : GameCommand, IMemoryPackable<SetQuestObjectiveViewedGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetQuestObjectiveViewedGameCommand>
{
	[Preserve]
	private sealed class SetQuestObjectiveViewedGameCommandFormatter : MemoryPackFormatter<SetQuestObjectiveViewedGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetQuestObjectiveViewedGameCommand value)
		{
			SetQuestObjectiveViewedGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetQuestObjectiveViewedGameCommand value)
		{
			SetQuestObjectiveViewedGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetQuestObjectiveViewedGameCommand value)
		{
			SetQuestObjectiveViewedGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetQuestObjectiveViewedGameCommand value)
		{
			SetQuestObjectiveViewedGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityFactRef<QuestObjective> m_QuestObjectiveRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetQuestObjectiveViewedGameCommand()
	{
	}

	[JsonConstructor]
	public SetQuestObjectiveViewedGameCommand(QuestObjective questObjective)
	{
		m_QuestObjectiveRef = questObjective;
	}

	protected override void ExecuteInternal()
	{
		QuestObjective fact = m_QuestObjectiveRef.Fact;
		if (fact != null)
		{
			fact.IsViewed = true;
		}
	}

	static SetQuestObjectiveViewedGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetQuestObjectiveViewedGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_QuestObjectiveRef", typeof(EntityFactRef<QuestObjective>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestObjectiveViewedGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetQuestObjectiveViewedGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestObjectiveViewedGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetQuestObjectiveViewedGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetQuestObjectiveViewedGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_QuestObjectiveRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetQuestObjectiveViewedGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityFactRef<QuestObjective> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_QuestObjectiveRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityFactRef<QuestObjective>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetQuestObjectiveViewedGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_QuestObjectiveRef : default(EntityFactRef<QuestObjective>));
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
		value = new SetQuestObjectiveViewedGameCommand
		{
			m_QuestObjectiveRef = value2
		};
		return;
		IL_0070:
		value.m_QuestObjectiveRef = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetQuestObjectiveViewedGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_QuestObjectiveRef");
		writer.WritePackable(value.m_QuestObjectiveRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetQuestObjectiveViewedGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityFactRef<QuestObjective> val = ((value != null) ? value.m_QuestObjectiveRef : default(EntityFactRef<QuestObjective>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_QuestObjectiveRef")
				{
					val = reader.ReadPackable<EntityFactRef<QuestObjective>>();
					array[0] = true;
				}
			}
			else if (text == "m_QuestObjectiveRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_QuestObjectiveRef = val;
		}
		else
		{
			value = new SetQuestObjectiveViewedGameCommand
			{
				m_QuestObjectiveRef = val
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
		SetQuestObjectiveViewedGameCommand source = new SetQuestObjectiveViewedGameCommand();
		result = Unsafe.As<SetQuestObjectiveViewedGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetQuestObjectiveViewedGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_QuestObjectiveRef", ref m_QuestObjectiveRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetQuestObjectiveViewedGameCommand>();
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
				m_QuestObjectiveRef = formatter.ReadPackable<EntityFactRef<QuestObjective>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
