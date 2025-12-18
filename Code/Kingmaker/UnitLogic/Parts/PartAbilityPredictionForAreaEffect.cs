using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartAbilityPredictionForAreaEffect : UnitPart, IHashable, IOwlPackable<PartAbilityPredictionForAreaEffect>
{
	private readonly List<(EntityFactComponent Runtime, SpawnAreaEffectOnAbilityCast Component)> m_PatternEntries = new List<(EntityFactComponent, SpawnAreaEffectOnAbilityCast)>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityPredictionForAreaEffect",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public OrientedPatternData? GetAreaEffectPatternNotFromPatternCenter(AbilityData ability, TargetWrapper target)
	{
		if (target == null)
		{
			return null;
		}
		MechanicEntity caster = ability.Caster;
		PartAbilityPredictionForAreaEffect partAbilityPredictionForAreaEffectOptional = caster.GetPartAbilityPredictionForAreaEffectOptional();
		if (partAbilityPredictionForAreaEffectOptional == null)
		{
			return null;
		}
		GridNodeBase nearestNode = target.NearestNode;
		GridNodeBase nearestNodeXZ = caster.GetNearestNodeXZ();
		GridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(nearestNodeXZ, nearestNode.Vector3Position());
		GridNodeBase outerNodeNearestToTarget = caster.GetOuterNodeNearestToTarget(nearestNodeXZ, nearestNode.Vector3Position());
		IAbilityAoEPatternProvider abilityAoEPatternProvider = null;
		bool flag = false;
		foreach (var patternEntry in partAbilityPredictionForAreaEffectOptional.m_PatternEntries)
		{
			using (patternEntry.Runtime.SetScope())
			{
				BlueprintAreaEffect blueprintAbilityAreaEffect = patternEntry.Component.GetBlueprintAbilityAreaEffect(ability);
				if (blueprintAbilityAreaEffect != null)
				{
					abilityAoEPatternProvider = blueprintAbilityAreaEffect;
					flag = patternEntry.Component.GetOrientationFromCaster;
					break;
				}
			}
		}
		if (abilityAoEPatternProvider?.Pattern == null)
		{
			return null;
		}
		Vector3 vector = AoEPattern.GetCastDirection(abilityAoEPatternProvider.Pattern.Type, innerNodeNearestToTarget, nearestNode, nearestNode);
		bool flag2 = flag && vector.sqrMagnitude < 1f;
		if (flag2)
		{
			vector = caster.Forward;
		}
		using (ProfileScope.New("GetOriented from PartPredictionForAreaEffect"))
		{
			Size targetSizeForPattern = ability.GetTargetSizeForPattern(target);
			GridNodeBase applicationNode = (flag2 ? (outerNodeNearestToTarget.Vector3Position() + vector * 1.Cells().Meters).GetNearestNodeXZUnwalkable() : outerNodeNearestToTarget);
			AoEPatternCalculation @params = new AoEPatternCalculation(innerNodeNearestToTarget, applicationNode, vector).SetIgnoreLos(abilityAoEPatternProvider.IsIgnoreLos).SetIgnoreLevelDifference(abilityAoEPatternProvider.IsIgnoreLevelDifference).SetDirectional(directional: true)
				.SetUseMeleeLos(abilityAoEPatternProvider.UseMeleeLos)
				.SetEntitySizeRect(targetSizeForPattern);
			return abilityAoEPatternProvider.Pattern.GetOriented(@params);
		}
	}

	public void Add(SpawnAreaEffectOnAbilityCast component)
	{
		m_PatternEntries.Add((component.Runtime, component));
	}

	public void Remove(SpawnAreaEffectOnAbilityCast component)
	{
		m_PatternEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_PatternEntries.Empty())
		{
			RemoveSelf();
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
		PartAbilityPredictionForAreaEffect source = new PartAbilityPredictionForAreaEffect();
		result = Unsafe.As<PartAbilityPredictionForAreaEffect, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityPredictionForAreaEffect>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityPredictionForAreaEffect>();
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
