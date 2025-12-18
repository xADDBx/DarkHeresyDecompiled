using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/CustomPositionListEvaluator")]
[TypeId("53ce4288036249abb6d55b16a48268e2")]
[OwlPackable(OwlPackableMode.Generate)]
public class CustomPositionListEvaluator : PositionListEvaluator, IOwlPackable, IOwlPackable<CustomPositionListEvaluator>
{
	[Serializable]
	private class PositionEvaluatorWrapper
	{
		[ValidateNotNull]
		[SerializeReference]
		public PositionEvaluator Position;
	}

	[SerializeField]
	private PositionEvaluatorWrapper[] m_Positions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CustomPositionListEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return "List of set up positions";
	}

	protected override List<Vector3> GetValueInternal()
	{
		List<Vector3> list = TempList.Get<Vector3>();
		PositionEvaluatorWrapper[] positions = m_Positions;
		foreach (PositionEvaluatorWrapper positionEvaluatorWrapper in positions)
		{
			if (positionEvaluatorWrapper?.Position != null)
			{
				list.Add(positionEvaluatorWrapper.Position.GetValue());
			}
		}
		return list;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CustomPositionListEvaluator source = new CustomPositionListEvaluator();
		result = Unsafe.As<CustomPositionListEvaluator, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CustomPositionListEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CustomPositionListEvaluator>();
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
