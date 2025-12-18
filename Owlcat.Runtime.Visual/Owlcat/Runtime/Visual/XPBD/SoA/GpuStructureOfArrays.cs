using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public abstract class GpuStructureOfArrays<T, S> : GpuStructureOfArraysBase where T : struct where S : StructureOfArrays<T>
{
	public GpuStructureOfArrays(int size)
		: base(size)
	{
	}

	public virtual void SetData(S data)
	{
	}

	public virtual void SetData(S data, int offset, int count)
	{
	}

	public virtual void SetData(CommandBuffer cmd, S data)
	{
	}

	public virtual void SetData(CommandBuffer cmd, S data, int offset, int count)
	{
	}
}
