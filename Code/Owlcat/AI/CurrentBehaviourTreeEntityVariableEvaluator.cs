using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Owlcat.AI;

[OwlPackable(OwlPackableMode.Generate)]
[ComponentName("AI/CurrentBehaviourTreeEntityVariableEvaluator")]
[TypeId("501d652c63ff44d5823c053882b94d95")]
public class CurrentBehaviourTreeEntityVariableEvaluator : MechanicEntityEvaluator, IOwlPackable<CurrentBehaviourTreeEntityVariableEvaluator>
{
	[SerializeField]
	private string VariableKey;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CurrentBehaviourTreeEntityVariableEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return "'" + VariableKey + "' from BehaviourTree";
	}

	protected override Entity GetValueInternal()
	{
		if (!(BehaviourTreeContext.Blackboard.Variables.FirstOrDefault((BlackboardVariable v) => v.Key == VariableKey) is EntityVariable entityVariable))
		{
			return null;
		}
		return entityVariable.Value;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CurrentBehaviourTreeEntityVariableEvaluator source = new CurrentBehaviourTreeEntityVariableEvaluator();
		result = Unsafe.As<CurrentBehaviourTreeEntityVariableEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CurrentBehaviourTreeEntityVariableEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CurrentBehaviourTreeEntityVariableEvaluator>();
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
