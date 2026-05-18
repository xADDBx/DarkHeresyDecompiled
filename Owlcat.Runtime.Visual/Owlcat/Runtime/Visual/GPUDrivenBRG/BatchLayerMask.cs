namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class BatchLayerMask
{
	public static uint FromLayers(BatchLayerFlagBits layerFlagBits)
	{
		return (uint)layerFlagBits;
	}
}
