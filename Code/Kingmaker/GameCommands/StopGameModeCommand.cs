using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameModes;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class StopGameModeCommand : ChangeGameModeCommand, IOwlPackable<StopGameModeCommand>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StopGameModeCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public StopGameModeCommand(GameModeType gameMode)
		: base(ActionType.Stop, gameMode)
	{
	}

	private StopGameModeCommand(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void ExecuteInternal()
	{
		((IGameDoStopMode)Game.Instance).DoStopMode(GameMode);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StopGameModeCommand source = new StopGameModeCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StopGameModeCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StopGameModeCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StopGameModeCommand>();
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
