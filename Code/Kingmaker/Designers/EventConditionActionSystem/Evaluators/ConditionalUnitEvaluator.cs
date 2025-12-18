using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("e2b7a7cc2b994e3c925a5ab6abc2b793")]
[OwlPackable(OwlPackableMode.Generate)]
public class ConditionalUnitEvaluator : AbstractUnitEvaluator, IOwlPackable<ConditionalUnitEvaluator>
{
	[Serializable]
	private class ConditionalPair
	{
		public ConditionsChecker Condition;

		[ValidateNotNull]
		[SerializeReference]
		public AbstractUnitEvaluator Unit;
	}

	[SerializeField]
	private ConditionalPair[] m_Units;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Default;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ConditionalUnitEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		ConditionalPair[] units = m_Units;
		foreach (ConditionalPair conditionalPair in units)
		{
			if ((bool)conditionalPair.Unit && conditionalPair.Condition.Check())
			{
				return conditionalPair.Unit.GetValue();
			}
		}
		if (!m_Default)
		{
			return null;
		}
		return m_Default.GetValue();
	}

	public override string GetCaption()
	{
		return "Unit selector";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ConditionalUnitEvaluator source = new ConditionalUnitEvaluator();
		result = Unsafe.As<ConditionalUnitEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConditionalUnitEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConditionalUnitEvaluator>();
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
