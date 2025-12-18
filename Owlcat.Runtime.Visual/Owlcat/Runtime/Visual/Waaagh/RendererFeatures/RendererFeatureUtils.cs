using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

public static class RendererFeatureUtils
{
	public static void DrawRendererWithGPUDrivenInstanceData(CommandBuffer cmd, Renderer renderer, Material material, int materialIndex, int shaderPass)
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((object)asset != null)
		{
			GPUDrivenBRGSettings gPUDrivenBRGSettings = asset.GPUDrivenBRGSettings;
			if (gPUDrivenBRGSettings != null && gPUDrivenBRGSettings.IsEnabledAndSupported && renderer is MeshRenderer && RendererUtils.AllowGPUDrivenRendering(renderer))
			{
				ReadOnlySpan<GPUDrivenRenderer.PropertyData> readOnlySpan = ReadOnlySpan<GPUDrivenRenderer.PropertyData>.Empty;
				ReadOnlySpan<GPUDrivenRenderer.PropertyData> readOnlySpan2;
				if (renderer.TryGetComponent<GPUDrivenRenderer>(out var component))
				{
					readOnlySpan = component.GetInstanceData();
					readOnlySpan2 = readOnlySpan;
					for (int i = 0; i < readOnlySpan2.Length; i++)
					{
						GPUDrivenRenderer.PropertyData propertyData = readOnlySpan2[i];
						SetPropertyData(cmd, in propertyData);
					}
				}
				cmd.DrawRenderer(renderer, material, materialIndex, 0);
				readOnlySpan2 = readOnlySpan;
				for (int i = 0; i < readOnlySpan2.Length; i++)
				{
					GPUDrivenRenderer.PropertyData propertyData2 = readOnlySpan2[i];
					GPUDrivenRenderer.PropertyValue propertyValue = default(GPUDrivenRenderer.PropertyValue);
					SetPropertyData(cmd, in propertyData2, in propertyValue);
				}
				return;
			}
		}
		cmd.DrawRenderer(renderer, material, materialIndex, 0);
	}

	private static void SetPropertyData(CommandBuffer cmd, in GPUDrivenRenderer.PropertyData propertyData)
	{
		SetPropertyData(cmd, in propertyData, in propertyData.Value);
	}

	private static void SetPropertyData(CommandBuffer cmd, in GPUDrivenRenderer.PropertyData propertyData, in GPUDrivenRenderer.PropertyValue propertyValue)
	{
		switch (propertyData.Type)
		{
		case GPUDrivenRenderer.PropertyDataType.Float:
			cmd.SetGlobalFloat(propertyData.NameID, propertyValue.Float);
			break;
		case GPUDrivenRenderer.PropertyDataType.Int:
			cmd.SetGlobalInt(propertyData.NameID, propertyValue.Int);
			break;
		case GPUDrivenRenderer.PropertyDataType.Vector:
			cmd.SetGlobalVector(propertyData.NameID, propertyValue.Vector);
			break;
		case GPUDrivenRenderer.PropertyDataType.Color:
			cmd.SetGlobalColor(propertyData.NameID, propertyValue.Color);
			break;
		case GPUDrivenRenderer.PropertyDataType.Matrix:
			cmd.SetGlobalMatrix(propertyData.NameID, propertyValue.Matrix);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
