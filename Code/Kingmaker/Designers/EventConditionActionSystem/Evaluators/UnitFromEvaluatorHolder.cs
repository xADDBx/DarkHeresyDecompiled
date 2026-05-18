using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[OwlPackable(OwlPackableMode.Generate)]
[TypeId("b41d5fedab27453da96f5f115df5ced7")]
public class UnitFromEvaluatorHolder : AbstractUnitEvaluator, IOwlPackable<UnitFromEvaluatorHolder>
{
	[SerializeField]
	public UnitEvaluatorHolderReference? Holder;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitFromEvaluatorHolder",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity? GetAbstractUnitEntityInternal()
	{
		return Holder?.Get()?.Evaluate();
	}

	public override string GetCaption()
	{
		return "Unit from evaluator holder (" + Holder.NameSafe() + ")";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitFromEvaluatorHolder source = new UnitFromEvaluatorHolder();
		result = Unsafe.As<UnitFromEvaluatorHolder, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitFromEvaluatorHolder>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitFromEvaluatorHolder>();
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
