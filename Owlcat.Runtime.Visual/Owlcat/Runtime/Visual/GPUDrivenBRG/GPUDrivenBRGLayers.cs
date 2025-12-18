namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenBRGLayers
{
	public const byte kNonBRGLayer = 0;

	public const byte kDefaultLayer = 1;

	public const byte kDepthOnlyLayer = 2;

	public const byte kMotionVectorsLayer = 3;

	public const byte kDebugLayer = 7;

	public const uint kDepthOnlyMask = 4u;

	public const uint kMotionVectorsMask = 8u;

	public const uint kGBufferMask = 4294967283u;

	public const uint kDepthPrePassMask = 5u;

	public const uint kDebugMask = 128u;

	public const uint kAllMask = 140u;
}
