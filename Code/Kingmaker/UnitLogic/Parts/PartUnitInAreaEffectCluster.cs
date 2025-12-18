using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitInAreaEffectCluster : BaseUnitPart, IHashable, IOwlPackable<PartUnitInAreaEffectCluster>
{
	private readonly HashSet<BlueprintAreaEffectClusterLogic> m_ClusterKeys = new HashSet<BlueprintAreaEffectClusterLogic>();

	private readonly Dictionary<BlueprintAreaEffectClusterLogic, HashSet<AreaEffectEntity>> m_AreaEffectEntitiesInVisit = new Dictionary<BlueprintAreaEffectClusterLogic, HashSet<AreaEffectEntity>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitInAreaEffectCluster",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public HashSet<BlueprintAreaEffectClusterLogic> ClusterKeys => m_ClusterKeys;

	public Dictionary<BlueprintAreaEffectClusterLogic, HashSet<AreaEffectEntity>> AreaEffectEntitiesInVisit => m_AreaEffectEntitiesInVisit;

	public void AddClusterKey(BlueprintAreaEffectClusterLogic blueprint)
	{
		m_ClusterKeys.Add(blueprint);
	}

	public void RemoveClusterKey(BlueprintAreaEffectClusterLogic blueprint)
	{
		m_ClusterKeys.Remove(blueprint);
		m_AreaEffectEntitiesInVisit.Remove(blueprint);
		RemoveSelfIfEmpty();
	}

	public void AddEnteringAreaEffectToList(BlueprintAreaEffectClusterLogic blueprint, AreaEffectEntity entity)
	{
		if (m_AreaEffectEntitiesInVisit.ContainsKey(blueprint))
		{
			m_AreaEffectEntitiesInVisit[blueprint].Add(entity);
			return;
		}
		m_AreaEffectEntitiesInVisit[blueprint] = new HashSet<AreaEffectEntity> { entity };
	}

	public void RemoveExitingAreaEffectFromList(BlueprintAreaEffectClusterLogic blueprint, AreaEffectEntity entity)
	{
		m_AreaEffectEntitiesInVisit[blueprint].Remove(entity);
	}

	private void RemoveSelfIfEmpty()
	{
		if (!base.Owner.IsDisposingNow && m_ClusterKeys.Empty())
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
		PartUnitInAreaEffectCluster source = new PartUnitInAreaEffectCluster();
		result = Unsafe.As<PartUnitInAreaEffectCluster, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitInAreaEffectCluster>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitInAreaEffectCluster>();
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
