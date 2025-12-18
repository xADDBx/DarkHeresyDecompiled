using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingPassData : PassDataBase
{
	public class UsedBuffers
	{
		public readonly List<BufferHandle> CPUInstanceVisibilityMasks = new List<BufferHandle>();

		public readonly List<BufferHandle> IndirectArgs = new List<BufferHandle>();

		public readonly List<BufferHandle> VisibleIndices = new List<BufferHandle>();

		public BufferHandle CullingJobs;

		public BufferHandle EmptyViewDependentLODData;

		public BufferHandle GroupInfo;

		public BufferHandle LODGroupData;

		public BufferHandle PersistentIndices;

		public BufferHandle VisibilityInfo;

		public void Clear()
		{
			IndirectArgs.Clear();
			VisibleIndices.Clear();
			CPUInstanceVisibilityMasks.Clear();
		}
	}

	public class UsedTextures
	{
		public TextureHandle CameraDepthBuffer;

		public TextureHandle CameraDepthPyramid;
	}

	public readonly UsedBuffers Buffers = new UsedBuffers();

	public readonly UsedTextures Textures = new UsedTextures();

	public GPUDrivenBatchRendererGroup BRG;

	public CameraType CameraType;
}
