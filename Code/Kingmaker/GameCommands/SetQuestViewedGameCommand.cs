using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SetQuestViewedGameCommand : GameCommand, IOwlPackable<SetQuestViewedGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<Quest> m_QuestRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SetQuestViewedGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_QuestRef", typeof(EntityFactRef<Quest>))
		}
	};

	public override bool IsSynchronized => true;

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
