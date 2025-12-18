namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenMaterialLayoutChangesExtensions
{
	private const GPUDrivenRenderingUtils.MaterialLayoutChangeFlags kDrasticChangesMask = GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Properties | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.TotalSizeIncreased;

	public static bool Any(this in GPUDrivenRenderingUtils.MaterialLayoutChanges changes)
	{
		if (changes.PerMaterialData == GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Nothing)
		{
			return changes.PerInstanceData != GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Nothing;
		}
		return true;
	}

	public static bool AreDrastic(this in GPUDrivenRenderingUtils.MaterialLayoutChanges changes)
	{
		if ((changes.PerMaterialData & (GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Properties | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.TotalSizeIncreased)) == 0)
		{
			return (changes.PerInstanceData & (GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Properties | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.TotalSizeIncreased)) != 0;
		}
		return true;
	}
}
