using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("5490dbe723156cb459fa639153196db0")]
[OwlPackable(OwlPackableMode.Generate)]
public class FactOwner : AbstractUnitEvaluator, IOwlPackable<FactOwner>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FactOwner",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return EvalContext.Current.Fact?.Owner as AbstractUnitEntity;
	}

	public override string GetCaption()
	{
		return "Fact Owner";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FactOwner source = new FactOwner();
		result = Unsafe.As<FactOwner, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FactOwner>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FactOwner>();
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
