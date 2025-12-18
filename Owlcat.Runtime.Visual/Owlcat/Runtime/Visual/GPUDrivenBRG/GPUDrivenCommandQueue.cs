using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenCommandQueue : IDisposable
{
	private static class Profiling
	{
		public static readonly ProfilingSampler Flush = new ProfilingSampler("Flush");
	}

	private readonly Queue<(IDisposable disposable, int frameIndex)> m_DeletionQueue = new Queue<(IDisposable, int)>();

	private int m_FrameIndex;

	public CommandBuffer Cmd { get; } = new CommandBuffer
	{
		name = "GPUDrivenBRG CommandQueue"
	};


	public void Dispose()
	{
		while (m_DeletionQueue.Count > 0)
		{
			m_DeletionQueue.Dequeue().disposable.Dispose();
		}
		m_DeletionQueue.Clear();
		Cmd.Dispose();
	}

	public void Flush(ScriptableRenderContext renderContext)
	{
		using (new ProfilingScope(Profiling.Flush))
		{
			if (Cmd.sizeInBytes != 0)
			{
				renderContext.ExecuteCommandBuffer(Cmd);
				Cmd.Clear();
			}
		}
	}

	public void PostRender()
	{
		(IDisposable, int) result;
		while (m_DeletionQueue.TryPeek(out result) && m_FrameIndex - result.Item2 >= 3)
		{
			result.Item1.Dispose();
			m_DeletionQueue.Dequeue();
		}
		m_FrameIndex++;
	}

	public void DeferDelete(IDisposable disposable)
	{
		m_DeletionQueue.Enqueue((disposable, m_FrameIndex));
	}
}
