using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("539f8515bfb9423d93dd3b3f791dee38")]
[OwlPackable(OwlPackableMode.Generate)]
public class ContextCasterUnit : AbstractUnitEvaluator, IOwlPackable<ContextCasterUnit>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ContextCasterUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return "Caster unit from mechanic context";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return EvalContext.Current.Caster as BaseUnitEntity;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ContextCasterUnit source = new ContextCasterUnit();
		result = Unsafe.As<ContextCasterUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ContextCasterUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ContextCasterUnit>();
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
