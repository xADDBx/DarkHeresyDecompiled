using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[OwlPackable(OwlPackableMode.Generate)]
public class PartSmoothCombatTransition : UnitPart, IHashable, IOwlPackable<PartSmoothCombatTransition>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartSmoothCombatTransition",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool IsEnabled { get; set; }

	public float TransitionDuration { get; set; } = 0.3f;


	public Vector3 CurrentViewPosition { get; set; }

	public float TransitionStartTime { get; set; } = -1f;


	public Vector3 TargetPosition { get; set; }

	public Vector3 GetSmoothPosition(Vector3 targetWithOffset)
	{
		if (!IsEnabled)
		{
			return targetWithOffset;
		}
		if (TransitionStartTime < 0f)
		{
			TransitionStartTime = Time.time;
			TargetPosition = targetWithOffset;
			PFLog.Default.Log($"[PartSmoothCombatTransition] Starting transition from {CurrentViewPosition} to {TargetPosition}");
		}
		float num = Mathf.Clamp01((Time.time - TransitionStartTime) / TransitionDuration);
		num = num * num * (3f - 2f * num);
		Vector3 result = Vector3.Lerp(CurrentViewPosition, TargetPosition, num);
		if (num >= 1f)
		{
			IsEnabled = false;
			PFLog.Default.Log("[PartSmoothCombatTransition] Transition completed");
			return TargetPosition;
		}
		return result;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartSmoothCombatTransition source = new PartSmoothCombatTransition();
		result = Unsafe.As<PartSmoothCombatTransition, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartSmoothCombatTransition>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartSmoothCombatTransition>();
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
