using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Code.Gameplay.Evaluators;

[OwlPackable(OwlPackableMode.Generate)]
[TypeId("811d37b9ee004341b03e11e1efa4f86f")]
public class CombatUnitIteratorEvaluator : AbstractUnitEvaluator, IOwlPackable<CombatUnitIteratorEvaluator>
{
	public ConditionsChecker Conditions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CombatUnitIteratorEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return "Find Unit By Condition from all Units in Combat";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			return null;
		}
		foreach (MechanicEntity item in Game.Instance.Controllers.TurnController.UnitsInCombat)
		{
			using (ContextData<CombatUnitData>.Request().Setup(item as BaseUnitEntity))
			{
				if (Conditions.Check())
				{
					return item as AbstractUnitEntity;
				}
			}
		}
		return null;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CombatUnitIteratorEvaluator source = new CombatUnitIteratorEvaluator();
		result = Unsafe.As<CombatUnitIteratorEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CombatUnitIteratorEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CombatUnitIteratorEvaluator>();
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
