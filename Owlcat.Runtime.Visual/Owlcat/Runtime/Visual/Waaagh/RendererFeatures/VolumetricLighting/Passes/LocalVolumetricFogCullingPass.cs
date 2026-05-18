using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

internal static class LocalVolumetricFogCullingPass
{
	private sealed class PassData
	{
		public Texture3DAtlas Atlas;

		public NativeArray<LocalVolumetricFogBounds> VisibleVolumeBoundsList;

		public NativeArray<LocalVolumetricFogEngineData> VisibleVolumeDataList;

		public NativeArray<ZBin> ZBins;

		public int VisibleVolumesCount;

		public BufferHandle VisibleVolumeBoundsBuffer;

		public BufferHandle VisibleVolumeDataBuffer;

		public BufferHandle FogTilesBuffer;

		public BufferHandle ZBinsBuffer;

		public int FogTilesBufferSize;

		public ComputeShader CullingShader;

		public ComputeShaderKernelDescriptor BuildFogTilesKernelDesc;

		public Vector4 ClusteringParams;

		public Matrix4x4 ScreenProjMatrix;

		public int3 DispatchSize;
	}

	public static void Record(in RecordContext context, VolumetricLightingRendererFeature feature, ComputeShader localVolumetricFogCullingShader, ComputeShaderKernelDescriptor buildFogTilesKernel, VolumetricLightingData volumetricLightingData)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("LocalVolumetricFogCulling", out passData, WaaaghProfileId.LocalVolumetricFogCulling.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\VolumetricLighting\\Passes\\LocalVolumetricFogCullingPass.cs", 42);
		Vector4 fogClusteringParams = feature.FogClusteringParams;
		int x = (int)(fogClusteringParams.x * fogClusteringParams.y);
		passData.ZBins = feature.ZBins;
		passData.VisibleVolumeDataList = feature.VisibleVolumeDataList;
		passData.VisibleVolumeBoundsList = feature.VisibleVolumeBoundsList;
		passData.VisibleVolumesCount = feature.VisibleVolumesCount;
		passData.VisibleVolumeDataBuffer = volumetricLightingData.VisibleVolumesDataBuffer;
		passData.VisibleVolumeBoundsBuffer = volumetricLightingData.VisibleVolumesBoundsBuffer;
		passData.ZBinsBuffer = volumetricLightingData.ZBinsBuffer;
		passData.FogTilesBuffer = volumetricLightingData.FogTilesBuffer;
		passData.FogTilesBufferSize = volumetricLightingData.FogTilesBufferCount;
		passData.Atlas = LocalVolumetricFogManager.Instance.VolumeAtlas;
		passData.CullingShader = feature.Resources.LocalVolumetricFogCullingCS;
		passData.BuildFogTilesKernelDesc = buildFogTilesKernel;
		passData.DispatchSize = new int3(RenderingUtils.DivRoundUp(x, (int)buildFogTilesKernel.ThreadGroupSize.x), 1, math.max(1, RenderingUtils.DivRoundUp(passData.VisibleVolumesCount, 32)));
		passData.ClusteringParams = fogClusteringParams;
		passData.ScreenProjMatrix = GetScreenProjMatrix(in context.CameraData);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._TilesMinMaxZTexture);
		unsafeRenderGraphBuilder.UseBuffer(in volumetricLightingData.VisibleVolumesDataBuffer);
		unsafeRenderGraphBuilder.UseBuffer(in volumetricLightingData.VisibleVolumesBoundsBuffer);
		unsafeRenderGraphBuilder.UseBuffer(in volumetricLightingData.ZBinsBuffer);
		unsafeRenderGraphBuilder.UseBuffer(in volumetricLightingData.FogTilesBuffer, AccessFlags.Write);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			data.Atlas.Update(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd));
			context.cmd.SetBufferData(data.VisibleVolumeDataBuffer, data.VisibleVolumeDataList, 0, 0, data.VisibleVolumesCount);
			context.cmd.SetBufferData(data.VisibleVolumeBoundsBuffer, data.VisibleVolumeBoundsList, 0, 0, data.VisibleVolumesCount);
			context.cmd.SetBufferData(data.ZBinsBuffer, data.ZBins.Reinterpret<float4>(Marshal.SizeOf<ZBin>()), 0, 0, 1024);
			context.cmd.SetComputeBufferParam(data.CullingShader, data.BuildFogTilesKernelDesc.Index, ShaderPropertyId._FogTilesBufferUAV, data.FogTilesBuffer);
			context.cmd.SetComputeIntParam(data.CullingShader, ShaderPropertyId._FogTilesBufferUAVSize, data.FogTilesBufferSize);
			context.cmd.SetComputeBufferParam(data.CullingShader, data.BuildFogTilesKernelDesc.Index, ShaderPropertyId._VisibleVolumeBoundsBuffer, data.VisibleVolumeBoundsBuffer);
			context.cmd.SetComputeIntParam(data.CullingShader, ShaderPropertyId._LocalFogVolumesCount, data.VisibleVolumesCount);
			context.cmd.SetComputeVectorParam(data.CullingShader, ShaderPropertyId._LocalVolumetricFogClusteringParams, data.ClusteringParams);
			context.cmd.SetComputeMatrixParam(data.CullingShader, ShaderPropertyId._ScreenProjMatrix, data.ScreenProjMatrix);
			context.cmd.DispatchCompute(data.CullingShader, data.BuildFogTilesKernelDesc.Index, data.DispatchSize.x, data.DispatchSize.y, data.DispatchSize.z);
		});
	}

	private static Matrix4x4 GetScreenProjMatrix(in WaaaghCameraData cameraData)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		float num = cameraData.cameraTargetDescriptor.width;
		float num2 = cameraData.cameraTargetDescriptor.height;
		matrix4x.SetRow(0, new Vector4(0.5f * num, 0f, 0f, 0.5f * num));
		matrix4x.SetRow(1, new Vector4(0f, 0.5f * num2, 0f, 0.5f * num2));
		matrix4x.SetRow(2, new Vector4(0f, 0f, 0.5f, 0.5f));
		matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		return matrix4x * cameraData.GetProjectionMatrix();
	}
}
