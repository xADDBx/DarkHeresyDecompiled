using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartPriorityTarget : BaseUnitPart, IAreaHandler, ISubscriber, IHashable, IOwlPackable<UnitPartPriorityTarget>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartPriorityTarget",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_PriorityTargets", typeof(List<EntityFactRef<Buff>>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<EntityFactRef<Buff>> m_PriorityTargets { get; set; } = new List<EntityFactRef<Buff>>();


	public void AddTarget(Buff buff)
	{
		RemoveTarget(buff);
		m_PriorityTargets.Add(buff);
	}

	public void RemoveTarget(Buff buff)
	{
		EntityFactRef<Buff> item = m_PriorityTargets.FirstItem((EntityFactRef<Buff> p) => p.Fact?.Blueprint == buff.Blueprint);
		if (!item.IsEmpty)
		{
			item.Fact?.Remove();
			m_PriorityTargets.Remove(item);
		}
	}

	public BaseUnitEntity GetPriorityTarget(BlueprintBuff buff)
	{
		return m_PriorityTargets.FirstItem((EntityFactRef<Buff> p) => p.Fact?.Blueprint == buff).Fact?.Owner;
	}

	public void OnAreaBeginUnloading()
	{
		foreach (EntityFactRef<Buff> priorityTarget in m_PriorityTargets)
		{
			priorityTarget.Fact?.Remove();
		}
		m_PriorityTargets.Clear();
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<EntityFactRef<Buff>> priorityTargets = m_PriorityTargets;
		if (priorityTargets != null)
		{
			for (int i = 0; i < priorityTargets.Count; i++)
			{
				EntityFactRef<Buff> obj = priorityTargets[i];
				Hash128 val2 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartPriorityTarget source = new UnitPartPriorityTarget();
		result = Unsafe.As<UnitPartPriorityTarget, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartPriorityTarget>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<EntityFactRef<Buff>> value = m_PriorityTargets;
		formatter.Field(0, "m_PriorityTargets", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartPriorityTarget>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_PriorityTargets = formatter.ReadPackable<List<EntityFactRef<Buff>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
