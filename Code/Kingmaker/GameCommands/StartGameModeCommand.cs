using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameModes;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class StartGameModeCommand : ChangeGameModeCommand, IOwlPackable<StartGameModeCommand>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StartGameModeCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public StartGameModeCommand(GameModeType gameMode)
		: base(ActionType.Start, gameMode)
	{
	}

	private StartGameModeCommand(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void ExecuteInternal()
	{
		((IGameDoStartMode)Game.Instance).DoStartMode(GameMode);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StartGameModeCommand source = new StartGameModeCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StartGameModeCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StartGameModeCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StartGameModeCommand>();
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
