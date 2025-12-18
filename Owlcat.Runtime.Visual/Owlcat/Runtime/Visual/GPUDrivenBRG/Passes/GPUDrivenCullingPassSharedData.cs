using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingPassSharedData : IDisposable
{
	public class UsedBuffers
	{
		public BufferHandle CullingContexts;

		public BufferHandle CullingStats;

		public BufferHandle FrustumPlanes;

		public BufferHandle GroupCounters;

		public BufferHandle MainViewOcclusionTestDebug;

		public BufferHandle ForwardReflectionProbeIndices;
	}

	public struct CPUCullingStatsData : IDisposable
	{
		public NativeArray<int> MainFrustumCulled;

		public NativeArray<int> ShadowFrustumCulled;

		public CPUCullingStatsData(Allocator allocator)
		{
			MainFrustumCulled = new NativeArray<int>(1, allocator);
			ShadowFrustumCulled = new NativeArray<int>(1, allocator);
		}

		public void Dispose()
		{
			MainFrustumCulled.Dispose();
			MainFrustumCulled = default(NativeArray<int>);
			ShadowFrustumCulled.Dispose();
			ShadowFrustumCulled = default(NativeArray<int>);
		}

		public void Reset()
		{
			MainFrustumCulled[0] = 0;
			ShadowFrustumCulled[0] = 0;
		}
	}

	public struct GPUCullingStatsData
	{
		public int MainVisible;

		public int MainFrustumCulled;

		public int MainOcclusionCulled;

		public int ShadowVisible;

		public int ShadowFrustumCulled;

		public int ShadowOcclusionCulled;
	}

	public readonly UsedBuffers Buffers = new UsedBuffers();

	public readonly Action<AsyncGPUReadbackRequest> ReadbackCullingStats;

	public readonly HashSet<int> UsedInstanceVisibilityMaskIndices = new HashSet<int>();

	public CPUCullingStatsData CPUCullingStats;

	public NativeArray<GPUDrivenCullingContext> CullingContexts;

	public GPUCullingStatsData GPUCullingStats;

	public int MaxRendererGroupSlicesPerView;

	public GPUDrivenCullingPassSharedData()
	{
		ReadbackCullingStats = delegate(AsyncGPUReadbackRequest request)
		{
			if (!request.hasError)
			{
				GPUCullingStats = request.GetData<GPUCullingStatsData>()[0];
			}
		};
		CPUCullingStats = new CPUCullingStatsData(Allocator.Persistent);
	}

	public void Dispose()
	{
		CPUCullingStats.Dispose();
	}
}
