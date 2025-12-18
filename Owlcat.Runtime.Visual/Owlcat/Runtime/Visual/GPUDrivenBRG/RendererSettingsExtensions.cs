namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class RendererSettingsExtensions
{
	public static GPUDrivenMetadataAuthoring.MetadataComponents GetMetadataComponents(this in GPUDrivenRendererGroupPool.RendererSettings rendererSettings)
	{
		GPUDrivenMetadataAuthoring.MetadataComponents metadataComponents = GPUDrivenMetadataAuthoring.MetadataComponents.Default;
		if ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightMaps) != 0)
		{
			metadataComponents |= GPUDrivenMetadataAuthoring.MetadataComponents.LightMaps;
		}
		if ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightProbes) != 0)
		{
			metadataComponents |= GPUDrivenMetadataAuthoring.MetadataComponents.LightProbes;
		}
		return metadataComponents;
	}
}
