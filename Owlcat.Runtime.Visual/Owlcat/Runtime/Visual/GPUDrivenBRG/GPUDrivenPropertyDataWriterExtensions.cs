namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenPropertyDataWriterExtensions
{
	public static void SkipAllOptionalPerInstanceData(this ref GPUDrivenPropertyDataWriter writer, in GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey)
	{
		GPUDrivenRendererGroupPool.RendererGroupFlags flags = rendererGroupKey.RendererSettings.Flags;
		if ((flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightMaps) != 0)
		{
			writer.SkipPropertyDataRaw<GPUDrivenMetadataAuthoring.LightMapsPerInstanceData>();
		}
		if ((flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightProbes) != 0)
		{
			writer.SkipPropertyDataRaw<GPUDrivenMetadataAuthoring.LightProbesPerInstanceData>();
		}
	}
}
