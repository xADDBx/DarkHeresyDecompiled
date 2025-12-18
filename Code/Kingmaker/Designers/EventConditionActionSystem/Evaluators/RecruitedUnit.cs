using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("6c997aeb1537f2d44a5d7abd8ea1ef0a")]
[OwlPackable(OwlPackableMode.Generate)]
public class RecruitedUnit : AbstractUnitEvaluator, IOwlPackable<RecruitedUnit>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RecruitedUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<RecruitedUnitData>.Current?.Unit;
	}

	public override string GetCaption()
	{
		return "Recruited Unit";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RecruitedUnit source = new RecruitedUnit();
		result = Unsafe.As<RecruitedUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RecruitedUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RecruitedUnit>();
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
