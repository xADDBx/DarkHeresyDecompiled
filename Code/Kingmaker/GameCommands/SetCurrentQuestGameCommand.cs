using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SetCurrentQuestGameCommand : GameCommand, IOwlPackable<SetCurrentQuestGameCommand>
{
	private EntityFactRef<Quest> m_QuestRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SetCurrentQuestGameCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public SetCurrentQuestGameCommand(Quest quest)
	{
		m_QuestRef = quest;
	}

	private SetCurrentQuestGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.SetCurrentQuest(m_QuestRef.Fact);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SetCurrentQuestGameCommand source = new SetCurrentQuestGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<SetCurrentQuestGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetCurrentQuestGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetCurrentQuestGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
