using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD;

public abstract class EntityAllocator<EntityType, AllocationType> : EntityAllocatorBase where EntityType : XPBDEntity where AllocationType : struct
{
	protected Dictionary<EntityType, int> m_EntityAllocationMap = new Dictionary<EntityType, int>();

	protected Dictionary<EntityType, ActionsHistory> m_EntityHistory = new Dictionary<EntityType, ActionsHistory>();

	protected HashSet<EntityType> m_AddedEntities = new HashSet<EntityType>();

	protected HashSet<EntityType> m_RemovedEntities = new HashSet<EntityType>();

	protected List<StructureOfArraysBase> m_SoAs = new List<StructureOfArraysBase>();

	public Dictionary<EntityType, int> EntityAllocationMap => m_EntityAllocationMap;

	internal HashSet<EntityType> AddedEntities => m_AddedEntities;

	internal HashSet<EntityType> RemovedEntities => m_RemovedEntities;

	public bool IsEmpty => m_EntityAllocationMap.Count == 0;

	public abstract StructureOfArrays<AllocationType> Allocations { get; }

	protected T CreateSoA<T>(int size, Func<int, T> creator) where T : StructureOfArraysBase
	{
		T val = creator(size);
		m_SoAs.Add(val);
		return val;
	}

	protected SingleArraySoA<T> CreateSingleArraySoA<T>(int size) where T : struct
	{
		SingleArraySoA<T> singleArraySoA = new SingleArraySoA<T>(size);
		m_SoAs.Add(singleArraySoA);
		return singleArraySoA;
	}

	public void RegisterEntity(EntityType entity)
	{
		ActionsHistory.ActionType action = ActionsHistory.ActionType.Register;
		PushHistory(entity, in action);
	}

	public void UnregisterEntity(EntityType entity)
	{
		ActionsHistory.ActionType action = ActionsHistory.ActionType.Unregister;
		PushHistory(entity, in action);
	}

	private void PushHistory(EntityType entity, in ActionsHistory.ActionType action)
	{
		if (!m_EntityHistory.TryGetValue(entity, out var value))
		{
			value = default(ActionsHistory);
		}
		value.Push(in action);
		m_EntityHistory[entity] = value;
	}

	public void Alloc()
	{
		bool flag = false;
		if (!TryAlloc())
		{
			flag = true;
			BeforeGrow?.Invoke();
			Grow();
			if (!TryAlloc())
			{
				throw new Exception("Can't allocate AllocationType after Grow");
			}
		}
		PushData();
		if (flag)
		{
			AfterGrow?.Invoke();
		}
	}

	public override void Build()
	{
		ProcessHistory();
		if (m_RemovedEntities.Count > 0 || m_AddedEntities.Count > 0)
		{
			BeforeAlloc?.Invoke();
		}
		if (m_RemovedEntities.Count > 0)
		{
			Free();
		}
		if (m_AddedEntities.Count > 0)
		{
			Alloc();
		}
		if (m_RemovedEntities.Count > 0 || m_AddedEntities.Count > 0)
		{
			BuildInternalData();
			UpdateIndices();
			AfterAlloc?.Invoke();
		}
		m_RemovedEntities.Clear();
		m_AddedEntities.Clear();
		m_EntityHistory.Clear();
	}

	private void ProcessHistory()
	{
		foreach (KeyValuePair<EntityType, ActionsHistory> item in m_EntityHistory)
		{
			bool num = m_EntityAllocationMap.ContainsKey(item.Key);
			ActionsHistory.ActionType last = item.Value.Last;
			if (num)
			{
				switch (last)
				{
				case ActionsHistory.ActionType.Register:
					if (item.Value.Count > 1)
					{
						m_AddedEntities.Add(item.Key);
						m_RemovedEntities.Add(item.Key);
					}
					else
					{
						UnityEngine.Debug.Log(item.Key.name + " Is already registered but you are trying to register it again.");
					}
					break;
				case ActionsHistory.ActionType.Unregister:
					m_RemovedEntities.Add(item.Key);
					break;
				}
			}
			else
			{
				switch (last)
				{
				case ActionsHistory.ActionType.Register:
					m_AddedEntities.Add(item.Key);
					break;
				}
			}
		}
	}

	private void UpdateIndices()
	{
		foreach (EntityType removedEntity in m_RemovedEntities)
		{
			removedEntity.IndexInSolver = -1;
		}
		foreach (KeyValuePair<EntityType, int> item in m_EntityAllocationMap)
		{
			item.Key.IndexInSolver = item.Value;
		}
	}

	protected virtual void BuildInternalData()
	{
		if (!m_IndicesMap.IsCreated || m_IndicesMap.Length != m_EntityAllocationMap.Count)
		{
			if (m_IndicesMap.IsCreated)
			{
				m_IndicesMap.Dispose();
			}
			m_IndicesMap = new NativeArray<int>(m_EntityAllocationMap.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		int num = 0;
		foreach (KeyValuePair<EntityType, int> item in m_EntityAllocationMap)
		{
			m_IndicesMap[num++] = item.Value;
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		foreach (StructureOfArraysBase soA in m_SoAs)
		{
			soA.Dispose();
		}
		m_SoAs.Clear();
	}

	public override MemoryStat GetMemoryStat()
	{
		MemoryStat result = default(MemoryStat);
		foreach (StructureOfArraysBase soA in m_SoAs)
		{
			result.Cpu += soA.GetSizeInBytes();
		}
		result.Cpu += Marshal.SizeOf<int>() * m_IndicesMap.Length;
		return result;
	}
}
