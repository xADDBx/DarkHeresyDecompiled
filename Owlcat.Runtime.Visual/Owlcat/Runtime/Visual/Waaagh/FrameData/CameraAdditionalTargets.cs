using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public struct CameraAdditionalTargets
{
	public TextureHandle DepthCopy;

	public TextureHandle ColorPyramid;

	public TextureHandle MotionVectors;

	public TextureHandle MotionVectorsDepth;

	public TextureHandle RawColorHistory;

	public TextureHandle RawDepthHistory;
}
