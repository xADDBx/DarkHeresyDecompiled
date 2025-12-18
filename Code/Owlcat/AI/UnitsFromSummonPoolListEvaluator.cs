using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/UnitsFromSummonPoolListEvaluator")]
[TypeId("d4c49aaa4c9440a2b5c13fdfe2e2ecf7")]
[OwlPackable(OwlPackableMode.Generate)]
public class UnitsFromSummonPoolListEvaluator : MechanicEntityListEvaluator, IOwlPackable<UnitsFromSummonPoolListEvaluator>
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	[SerializeField]
	private ConditionsChecker m_Conditions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitsFromSummonPoolListEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	private BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetCaption()
	{
		return $"Units from {SummonPool}";
	}

	protected override List<Entity> GetValueInternal()
	{
		List<Entity> list = new List<Entity>();
		IEnumerable<AbstractUnitEntity> enumerable = Game.Instance.SummonPools.Get(SummonPool)?.Units;
		if (enumerable == null)
		{
			return list;
		}
		foreach (AbstractUnitEntity item in enumerable)
		{
			using (ContextData<SummonPoolUnitData>.Request().Setup(item))
			{
				if (m_Conditions.Check())
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitsFromSummonPoolListEvaluator source = new UnitsFromSummonPoolListEvaluator();
		result = Unsafe.As<UnitsFromSummonPoolListEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitsFromSummonPoolListEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitsFromSummonPoolListEvaluator>();
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
