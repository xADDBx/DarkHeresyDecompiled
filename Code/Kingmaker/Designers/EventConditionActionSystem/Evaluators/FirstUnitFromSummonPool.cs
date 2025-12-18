using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("d182d1ff68a8efd45bba3d39c75f5d8d")]
[PlayerUpgraderAllowed(true)]
[OwlPackable(OwlPackableMode.Generate)]
public class FirstUnitFromSummonPool : AbstractUnitEvaluator, IOwlPackable<FirstUnitFromSummonPool>
{
	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	public ConditionsChecker Conditions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FirstUnitFromSummonPool",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return null;
		}
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			using (ContextData<SummonPoolUnitData>.Request().Setup(unit))
			{
				if (unit.WillBeDestroyed || !Conditions.Check())
				{
					continue;
				}
				return unit;
			}
		}
		return null;
	}

	public override string GetCaption()
	{
		return $"First unit from {SummonPool}";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FirstUnitFromSummonPool source = new FirstUnitFromSummonPool();
		result = Unsafe.As<FirstUnitFromSummonPool, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FirstUnitFromSummonPool>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FirstUnitFromSummonPool>();
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
