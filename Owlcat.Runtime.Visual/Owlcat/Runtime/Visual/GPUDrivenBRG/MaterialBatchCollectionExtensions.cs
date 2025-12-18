using System;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class MaterialBatchCollectionExtensions
{
	public static GPUDrivenResourceRegistry.MaterialBatchCollection.Batch Get(this in GPUDrivenResourceRegistry.MaterialBatchCollection materialBatchCollection, in GPUDrivenMetadataAuthoring.MetadataComponents metadataComponents)
	{
		return metadataComponents switch
		{
			GPUDrivenMetadataAuthoring.MetadataComponents.Default => materialBatchCollection.Default, 
			GPUDrivenMetadataAuthoring.MetadataComponents.LightMaps => materialBatchCollection.LightMaps, 
			GPUDrivenMetadataAuthoring.MetadataComponents.LightProbes => materialBatchCollection.LightProbes, 
			_ => throw new ArgumentOutOfRangeException("metadataComponents", "Invalid component combination"), 
		};
	}
}
