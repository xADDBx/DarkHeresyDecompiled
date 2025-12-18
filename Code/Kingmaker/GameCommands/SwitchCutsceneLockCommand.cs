using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SwitchCutsceneLockCommand : GameCommand, IOwlPackable<SwitchCutsceneLockCommand>
{
	public readonly bool Lock;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SwitchCutsceneLockCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public SwitchCutsceneLockCommand(bool @lock)
	{
		Lock = @lock;
	}

	private SwitchCutsceneLockCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		((IGameDoSwitchCutsceneLock)Game.Instance).DoSwitchCutsceneLock(Lock);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SwitchCutsceneLockCommand source = new SwitchCutsceneLockCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<SwitchCutsceneLockCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SwitchCutsceneLockCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SwitchCutsceneLockCommand>();
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
