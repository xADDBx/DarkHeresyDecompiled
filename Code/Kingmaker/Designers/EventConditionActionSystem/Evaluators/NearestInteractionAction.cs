using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("d15d4796506e447f831fd0a47e3e4178")]
[PlayerUpgraderAllowed(true)]
[OwlPackable(OwlPackableMode.Generate)]
public class NearestInteractionAction : InteractionActionEvaluator, IOwlPackable, IOwlPackable<NearestInteractionAction>
{
	[Serializable]
	private class InteractionActionEvaluatorEntryWrapper
	{
		[SerializeReference]
		public InteractionActionEvaluator Interaction;
	}

	[SerializeField]
	[SerializeReference]
	private InteractionActionEvaluatorEntryWrapper[] m_Interactions;

	[SerializeField]
	private bool CheckInteractableOnly = true;

	[SerializeField]
	[SerializeReference]
	private PositionEvaluator m_Position;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NearestInteractionAction",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override InteractionActionPart GetValueInternal()
	{
		Vector3 value = m_Position.GetValue();
		InteractionActionPart result = null;
		float num = float.PositiveInfinity;
		InteractionActionEvaluatorEntryWrapper[] interactions = m_Interactions;
		for (int i = 0; i < interactions.Length; i++)
		{
			InteractionActionPart value2 = interactions[i].Interaction.GetValue();
			if (!CheckInteractableOnly || value2.CanInteract())
			{
				float sqrMagnitude = (value - value2.Owner.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = value2;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public override string GetCaption()
	{
		return $"Nearest InteractionAction from {m_Position}";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NearestInteractionAction source = new NearestInteractionAction();
		result = Unsafe.As<NearestInteractionAction, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<NearestInteractionAction>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NearestInteractionAction>();
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
