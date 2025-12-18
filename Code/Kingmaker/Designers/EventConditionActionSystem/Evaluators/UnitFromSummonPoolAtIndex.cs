using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("9d5137eb894c09141942280f5aa15427")]
[OwlPackable(OwlPackableMode.Generate)]
public class UnitFromSummonPoolAtIndex : AbstractUnitEvaluator, IOwlPackable<UnitFromSummonPoolAtIndex>
{
	[SerializeReference]
	public IntEvaluator Index;

	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitFromSummonPoolAtIndex",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetDescription()
	{
		return "Возвращает юнита из саммон пула по индексу, начиная с 0. Дизайнер, использующий этот эвалюатор, должен гарантировать, что юнит под таким индексом существует";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return null;
		}
		int value = Index.GetValue();
		if (summonPool.Units.Count() <= value)
		{
			return null;
		}
		return summonPool.Units.ToList()[value] as BaseUnitEntity;
	}

	public override string GetCaption()
	{
		return $"{Index} unit from {SummonPool}";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitFromSummonPoolAtIndex source = new UnitFromSummonPoolAtIndex();
		result = Unsafe.As<UnitFromSummonPoolAtIndex, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitFromSummonPoolAtIndex>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitFromSummonPoolAtIndex>();
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
