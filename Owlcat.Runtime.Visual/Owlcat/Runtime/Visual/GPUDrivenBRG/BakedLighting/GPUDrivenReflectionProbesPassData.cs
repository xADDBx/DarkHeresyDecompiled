using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;

public class GPUDrivenReflectionProbesPassData : PassDataBase
{
	public struct UsedBuffers
	{
		public BufferHandle PersistentIndices;

		public BufferHandle VisibilityInfo;

		public BufferHandle CullingContexts;

		public BufferHandle CullingJobs;

		public BufferHandle GroupInfo;

		public BufferHandle GroupCounters;

		public BufferHandle ProbeIndices;
	}

	public GPUDrivenBatchRendererGroup BRG;

	public UsedBuffers Buffers;

	public int CullingContextIndex;

	public int CullingJobsCount;
}
