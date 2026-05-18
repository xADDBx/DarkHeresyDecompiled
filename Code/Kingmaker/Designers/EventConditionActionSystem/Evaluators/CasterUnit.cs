using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("56703b87cc0c36347937d201cc076a6d")]
[OwlPackable(OwlPackableMode.Generate)]
public class CasterUnit : AbstractUnitEvaluator, IOwlPackable<CasterUnit>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CasterUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return EvalContext.Current.Caster as BaseUnitEntity;
	}

	public override string GetCaption()
	{
		return "Caster Unit";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CasterUnit source = new CasterUnit();
		result = Unsafe.As<CasterUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CasterUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CasterUnit>();
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
