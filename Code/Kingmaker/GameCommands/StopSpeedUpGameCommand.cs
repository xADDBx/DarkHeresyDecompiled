using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class StopSpeedUpGameCommand : GameCommand, IOwlPackable<StopSpeedUpGameCommand>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StopSpeedUpGameCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public StopSpeedUpGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.StopSpeedUp();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StopSpeedUpGameCommand source = new StopSpeedUpGameCommand();
		result = Unsafe.As<StopSpeedUpGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StopSpeedUpGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StopSpeedUpGameCommand>();
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
