using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Commands;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class ClearAreaTransitionGroupGameCommand : GameCommand, IOwlPackable<ClearAreaTransitionGroupGameCommand>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ClearAreaTransitionGroupGameCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public ClearAreaTransitionGroupGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.GroupCommands.ClearDuplicates(typeof(AreaTransitionGroupCommand));
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ClearAreaTransitionGroupGameCommand source = new ClearAreaTransitionGroupGameCommand();
		result = Unsafe.As<ClearAreaTransitionGroupGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ClearAreaTransitionGroupGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ClearAreaTransitionGroupGameCommand>();
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
