using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture;

[Serializable]
public class VirtualTextureSettings
{
	[Range(1f, 1594f)]
	public int GPUAtlasSizeInMegaBytes = 128;

	public TilesInBatchLimit TilesInBatchLimit = TilesInBatchLimit.x16;

	[Range(1f, 8f)]
	public int MaxBatchesPerFrame = 1;

	public FeedbackMipBiasSettings FeedbackMipBiasSettings;

	public bool PreloadSmallestMips = true;

	public bool UseAsyncReadManager;

	public VTFeedbackResolveMode FeedbackResolveMode;
}
