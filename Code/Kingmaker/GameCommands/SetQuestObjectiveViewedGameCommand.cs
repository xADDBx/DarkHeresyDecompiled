using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SetQuestObjectiveViewedGameCommand : GameCommand, IOwlPackable<SetQuestObjectiveViewedGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<QuestObjective> m_QuestObjectiveRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SetQuestObjectiveViewedGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_QuestObjectiveRef", typeof(EntityFactRef<QuestObjective>))
		}
	};

	public override bool IsSynchronized => true;

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
