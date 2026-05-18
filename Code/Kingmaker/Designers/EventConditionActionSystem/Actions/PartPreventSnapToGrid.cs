using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[OwlPackable(OwlPackableMode.Generate)]
public class PartPreventSnapToGrid : UnitPart, ITurnBasedModeHandler, ISubscriber, IHashable, IOwlPackable<PartPreventSnapToGrid>
{
	private Vector3 m_StartViewPosition;

	private float m_TransitionStartTime;

	private bool m_IsTransitioning;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartPreventSnapToGrid",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool ShouldPreventSnapToGrid { get; set; }

	public bool UseCombatOffset { get; set; }

	public bool SmoothCombatTransition { get; set; }

	public float TransitionDuration { get; set; } = 0.3f;


	public Vector3 TargetViewPosition { get; set; }

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased && SmoothCombatTransition)
		{
			UnitEntity owner = base.Owner;
			if (owner?.View != null)
			{
				m_StartViewPosition = owner.View.transform.position;
				m_TransitionStartTime = Time.time;
				m_IsTransitioning = true;
				PFLog.Default.Log($"[SmoothCombatTransition] Starting transition for {owner.View.name} from {m_StartViewPosition} to {TargetViewPosition}");
				owner.View.StartCoroutine(SmoothTransitionCoroutine(owner));
			}
		}
	}

	private IEnumerator SmoothTransitionCoroutine(UnitEntity unit)
	{
		while (m_IsTransitioning && unit?.View != null)
		{
			float num = Mathf.Clamp01((Time.time - m_TransitionStartTime) / TransitionDuration);
			num = num * num * (3f - 2f * num);
			Vector3 position = Vector3.Lerp(m_StartViewPosition, TargetViewPosition, num);
			unit.View.transform.position = position;
			if (num >= 1f)
			{
				m_IsTransitioning = false;
				unit.View.InterpolationHelper?.ForceUpdatePosition(TargetViewPosition, unit.Orientation);
				PFLog.Default.Log("[SmoothCombatTransition] Completed transition for " + unit.View.name);
				SmoothCombatTransition = false;
			}
			yield return null;
		}
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
		PartPreventSnapToGrid source = new PartPreventSnapToGrid();
		result = Unsafe.As<PartPreventSnapToGrid, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartPreventSnapToGrid>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartPreventSnapToGrid>();
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
