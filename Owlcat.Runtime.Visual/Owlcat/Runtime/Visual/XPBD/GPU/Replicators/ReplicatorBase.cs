using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.GPU.Replicators;

public abstract class ReplicatorBase : IDisposable, IMemoryCounter
{
	private List<GpuStructureOfArraysBase> m_SoAs = new List<GpuStructureOfArraysBase>();

	public abstract bool IsEmpty { get; }

	public virtual void Dispose()
	{
		foreach (GpuStructureOfArraysBase soA in m_SoAs)
		{
			soA.Dispose();
		}
		m_SoAs.Clear();
	}

	protected T CreateSoA<T>(int size, Func<int, T> creator) where T : GpuStructureOfArraysBase
	{
		T val = creator(size);
		m_SoAs.Add(val);
		return val;
	}

	protected GpuSingleArraySoA<T> CreateSingleArraySoA<T>(string name, int size) where T : struct
	{
		GpuSingleArraySoA<T> gpuSingleArraySoA = new GpuSingleArraySoA<T>(name, size);
		m_SoAs.Add(gpuSingleArraySoA);
		return gpuSingleArraySoA;
	}

	public abstract bool Replicate(CommandBuffer cmd);

	public virtual MemoryStat GetMemoryStat()
	{
		MemoryStat result = default(MemoryStat);
		result.Cpu = 0;
		foreach (GpuStructureOfArraysBase soA in m_SoAs)
		{
			result.Gpu += soA.GetSizeInBytes();
		}
		return result;
	}
}
public abstract class ReplicatorBase<T> : ReplicatorBase where T : EntityAllocatorBase
{
	public T Allocator { get; private set; }

	public ReplicatorBase(T allocator)
	{
		Allocator = allocator;
		T allocator2 = Allocator;
		allocator2.AfterAlloc = (Action)Delegate.Combine(allocator2.AfterAlloc, new Action(OnAfterAlloc));
	}

	public override void Dispose()
	{
		base.Dispose();
		T allocator = Allocator;
		allocator.AfterAlloc = (Action)Delegate.Remove(allocator.AfterAlloc, new Action(OnAfterAlloc));
	}

	protected abstract void OnAfterAlloc();
}
