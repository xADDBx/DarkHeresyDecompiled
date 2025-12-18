using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingPreparePassData : PassDataBase
{
	public class UsedBuffers
	{
		public BufferHandle InstancesCreatedThisFrame;
	}

	public readonly UsedBuffers Buffers = new UsedBuffers();

	public GPUDrivenBatchRendererGroup BRG;
}
