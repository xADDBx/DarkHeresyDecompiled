using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;
using Kingmaker.View.Spawners.Components;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[TypeId("f4b9e02f16415fc458251991b5212df2")]
[OwlPackable(OwlPackableMode.Generate)]
public class CorpseInteractionEvaluator : MapObjectEvaluator, IOwlPackable<CorpseInteractionEvaluator>
{
	[AllowedEntityType(typeof(UnitSpawner))]
	[ValidateNotEmpty]
	[SerializeField]
	private EntityReference m_UnitSpawner;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CorpseInteractionEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override MapObjectEntity GetMapObjectInternal()
	{
		return (((m_UnitSpawner.FindData() as UnitSpawnerBase.MyData)?.SpawnedUnit)?.Entity.ToAbstractUnitEntity().Parts.GetOptional<SpawnerCorpseInteraction.CorpseInteractionPart>())?.InteractionObjectRef.Entity;
	}

	public override string GetCaption()
	{
		return "Corpse Interaction from" + m_UnitSpawner;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CorpseInteractionEvaluator source = new CorpseInteractionEvaluator();
		result = Unsafe.As<CorpseInteractionEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CorpseInteractionEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CorpseInteractionEvaluator>();
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
