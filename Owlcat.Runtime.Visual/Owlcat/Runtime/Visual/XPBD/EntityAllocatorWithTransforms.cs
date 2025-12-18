using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD;

public abstract class EntityAllocatorWithTransforms<EntityType, AllocationType> : EntityAllocator<EntityType, AllocationType> where EntityType : XPBDEntity where AllocationType : struct
{
	protected TransformAccessArray m_Transforms;

	public TransformAccessArray Transforms => m_Transforms;

	protected override void BuildInternalData()
	{
		if (m_Transforms.isCreated)
		{
			m_Transforms.Dispose();
		}
		m_Transforms = new TransformAccessArray(m_EntityAllocationMap.Count);
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
			m_Transforms.Add(item.Key.GetTransform());
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		if (m_Transforms.isCreated)
		{
			m_Transforms.Dispose();
		}
	}

	public override MemoryStat GetMemoryStat()
	{
		MemoryStat memoryStat = base.GetMemoryStat();
		if (m_Transforms.isCreated)
		{
			memoryStat.Cpu += Marshal.SizeOf<TransformAccess>() * m_Transforms.capacity;
		}
		return memoryStat;
	}
}
