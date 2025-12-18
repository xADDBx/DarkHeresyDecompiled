using System;
using Owlcat.Runtime.Visual.VirtualTexture;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class VirtualTextureDebug
{
	public bool ShowFeedback;

	[Range(0f, 1f)]
	public float FeedbackScale;

	public bool DontLoadFeedback;

	public bool ShowPhysicalAtlas;

	public bool ShowPhysicalAtlasSliceGrid;

	[Range(0f, 1f)]
	public float PhyscalAtlasScale;

	public SliceResolution PhysicalAtlasMaxSliceResolution;

	public bool ShowBatchedCopyRt;

	public float BatchedCopyRtScale;

	public bool ShowIndirectionTexture;

	[Range(0f, 1f)]
	public float IndirectTextureScale;

	public DebugTilesMode DebugTilesMode;

	public bool ShowVirtualAtlas;

	public float VirtualAtlasScale;

	public VirtualTextureMaterialUpdateMode MaterialUpdateMode;
}
