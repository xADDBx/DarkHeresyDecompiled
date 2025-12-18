using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SkipBarkGameCommand : GameCommand, IOwlPackable<SkipBarkGameCommand>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SkipBarkGameCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override bool IsSynchronized => true;

	protected override void ExecuteInternal()
	{
		CutsceneController.SkipBarkBanter();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SkipBarkGameCommand source = new SkipBarkGameCommand();
		result = Unsafe.As<SkipBarkGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SkipBarkGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SkipBarkGameCommand>();
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
