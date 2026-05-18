using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("a4531c768de041ac9c42d27b1b0d5c69")]
[PlayerUpgraderAllowed(true)]
[OwlPackable(OwlPackableMode.Generate)]
public class AreaEffectEvaluator : MechanicEntityEvaluator, IOwlPackable<AreaEffectEvaluator>
{
	[SerializeField]
	private BlueprintAreaEffectReference m_AreaEffect;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaEffectEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return m_AreaEffect?.NameSafe();
	}

	protected override Entity GetValueInternal()
	{
		BlueprintAreaEffect bpAreaEffect = m_AreaEffect?.Blueprint;
		if (bpAreaEffect == null)
		{
			return null;
		}
		return Game.Instance.EntityPools.AreaEffects.FirstOrDefault((AreaEffectEntity ae) => ae.Blueprint == bpAreaEffect);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaEffectEvaluator source = new AreaEffectEvaluator();
		result = Unsafe.As<AreaEffectEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaEffectEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaEffectEvaluator>();
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
