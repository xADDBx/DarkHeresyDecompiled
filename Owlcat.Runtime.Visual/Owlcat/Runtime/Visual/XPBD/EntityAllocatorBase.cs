using System;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD;

public abstract class EntityAllocatorBase : IMemoryCounter
{
	public Action BeforeAlloc;

	public Action AfterAlloc;

	public Action BeforeGrow;

	public Action AfterGrow;

	protected NativeArray<int> m_IndicesMap;

	public NativeArray<int> IndicesMap => m_IndicesMap;

	protected abstract void PushData();

	protected abstract bool TryAlloc();

	protected abstract void Grow();

	public abstract void Free();

	public virtual void Build()
	{
	}

	public virtual void Dispose()
	{
		if (m_IndicesMap.IsCreated)
		{
			m_IndicesMap.Dispose();
		}
	}

	public abstract MemoryStat GetMemoryStat();
}
