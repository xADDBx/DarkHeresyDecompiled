namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenViewTypeExtensions
{
	public static bool IsMainView(this GPUDrivenRendererGroupPool.ViewType viewType)
	{
		if (viewType != 0 && viewType != GPUDrivenRendererGroupPool.ViewType.DepthOnly)
		{
			return viewType == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors;
		}
		return true;
	}
}
