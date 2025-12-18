using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("a46af28f03764ce581de0af75e60ae67")]
[OwlPackable(OwlPackableMode.Generate)]
public class MechanicLightRootEvaluator : MechanicEntityEvaluator, IOwlPackable<MechanicLightRootEvaluator>
{
	[AllowedEntityType(typeof(MechanicLightRoot))]
	[ValidateNotEmpty]
	public EntityReference MechanicLightRoot;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicLightRootEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override Entity GetValueInternal()
	{
		MechanicLightRootView mechanicLightRootView = MechanicLightRoot.FindView() as MechanicLightRootView;
		if (!(mechanicLightRootView != null))
		{
			return null;
		}
		return mechanicLightRootView.Data;
	}

	public override string GetCaption()
	{
		return "MechanicLightRoot object";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicLightRootEvaluator source = new MechanicLightRootEvaluator();
		result = Unsafe.As<MechanicLightRootEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicLightRootEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicLightRootEvaluator>();
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
