using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[TypeId("69a4e9c15933491a9c28ef40e0800630")]
[OwlPackable(OwlPackableMode.Generate)]
public class FamiliarEvaluator : AbstractFamiliarEvaluator, IOwlPackable<FamiliarEvaluator>
{
	[NotNull]
	[SerializeReference]
	[AllowedEntityType(typeof(BaseUnitEntity))]
	public AbstractUnitEvaluator LeaderEvaluator;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FamiliarEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override BaseUnitEntity Leader => LeaderEvaluator.GetValue() as BaseUnitEntity;

	public override string GetCaption()
	{
		return "Familiar of " + LeaderEvaluator?.GetCaption();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FamiliarEvaluator source = new FamiliarEvaluator();
		result = Unsafe.As<FamiliarEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FamiliarEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FamiliarEvaluator>();
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
