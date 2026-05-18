using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("617887ceade1439a909ca130150ed7e2")]
[OwlPackable(OwlPackableMode.Generate)]
public class ContextCasterEvaluator : MechanicEntityEvaluator, IOwlPackable<ContextCasterEvaluator>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ContextCasterEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return "Evaluate caster of Context";
	}

	protected override Entity GetValueInternal()
	{
		return EvalContext.Current.Caster;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ContextCasterEvaluator source = new ContextCasterEvaluator();
		result = Unsafe.As<ContextCasterEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ContextCasterEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ContextCasterEvaluator>();
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
