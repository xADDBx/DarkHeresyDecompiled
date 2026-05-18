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
[TypeId("9b05ad74824a436ebe056cfc6306c9b9")]
[OwlPackable(OwlPackableMode.Generate)]
public class ContextTargetEntityEvaluator : MechanicEntityEvaluator, IOwlPackable<ContextTargetEntityEvaluator>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ContextTargetEntityEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return "Evaluate target entity of Context";
	}

	protected override Entity GetValueInternal()
	{
		return EvalContext.Current.Target?.Entity;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ContextTargetEntityEvaluator source = new ContextTargetEntityEvaluator();
		result = Unsafe.As<ContextTargetEntityEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ContextTargetEntityEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ContextTargetEntityEvaluator>();
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
