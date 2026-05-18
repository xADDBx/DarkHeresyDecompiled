using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[TypeId("608eb5cb25a9fde4db70341f4ea962b9")]
[OwlPackable(OwlPackableMode.Generate)]
public class Trap : MapObjectEvaluator, IOwlPackable<Trap>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Trap",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override MapObjectEntity GetMapObjectInternal()
	{
		return ContextData<BlueprintTrap.ElementsData>.Current?.TrapObject;
	}

	public override string GetCaption()
	{
		return "Trap";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Trap source = new Trap();
		result = Unsafe.As<Trap, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Trap>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Trap>();
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
