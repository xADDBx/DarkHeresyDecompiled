using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class DeferredLighting
{
	private class TilesMinMaxZPassData
	{
		public ComputeShader Shader;

		public TextureHandle TilesMinMaxZTexture;

		public bool MaxDepthOnly;

		public int3 DispatchSize;

		public int TileSize;
	}

	private class SetupLightDataPassData
	{
		public BufferHandle LightDataConstantBufferHandle;

		public BufferHandle LightVolumeDataConstantBufferHandle;

		public NativeArray<float4> LightDataRaw;

		public NativeArray<float4> LightVolumeDataRaw;

		public Vector4 LightDataParams;

		public Vector4 ClusteringParams;
	}

	private class LightCullingPassData
	{
		public ComputeShader LightCullingShader;

		public int BuildLightTilesKernel;

		public BufferHandle LightTilesBuffer;

		public int3 DispatchSize;

		public Matrix4x4 ScreenProjMatrix;

		public int LightTilesBufferSize;

		public TextureHandle TilesMinMaxZTexture;
	}

	private class LightingPassRasterData
	{
		public Material Material;

		public Color GlossyEnvironmentColor;

		public Color GlossyBlackColor;
	}

	private class BuildVariantsPassData
	{
		public BufferHandle FeatureTilesBuffer;

		public BufferHandle FeatureTilesListsBuffer;

		public BufferHandle IndirectArgsBuffer;

		public int TilesCountX;

		public int TilesCountY;

		public ComputeShader BuildFeatureTilesShader;

		public ComputeShader ClearIndirectArgsShader;

		public ComputeShader BuildFeatureVariantsShader;

		public int BuildFeatureTilesKernel;

		public int ClearIndirectArgsKernel;

		public int BuildFeatureVariantsKernel;
	}

	private class LightingPassComputeData
	{
		public ComputeShader Shader;

		public ShadowQuality ShadowQuality;

		public Color GlossyEnvironmentColor;

		public TextureHandle CameraColorRT;

		public bool IsCameraColorSrgb;

		public int2 CameraColorSize;

		public int2 FeatureTilesDimensions;

		public BufferHandle FeatureTilesBuffer;

		public BufferHandle IndirectArgsBuffer;

		public BufferHandle FeatureTilesListsBuffer;
	}

	private class DeferredFogPassData
	{
		public Material Material;
	}

	private static readonly int _TilesMinMaxZUAV = Shader.PropertyToID("_TilesMinMaxZUAV");

	private static readonly string MINMAXZ_TILE_SIZE_8 = "MINMAXZ_TILE_SIZE_8";

	private static readonly string MINMAXZ_TILE_SIZE_16 = "MINMAXZ_TILE_SIZE_16";

	private static readonly string MINMAXZ_TILE_SIZE_32 = "MINMAXZ_TILE_SIZE_32";

	private static readonly string MAX_ONLY = "MAX_ONLY";

	private static readonly int s_FeatureTiles = Shader.PropertyToID("_FeatureTiles");

	private static readonly int s_DispatchIndirectArgs = Shader.PropertyToID("_DispatchIndirectArgs");

	private static readonly int s_FeatureTilesCount = Shader.PropertyToID("_FeatureTilesCount");

	private static readonly int s_FeatureTilesCountX = Shader.PropertyToID("_FeatureTilesCountX");

	private static readonly int s_FeatureTilesLists = Shader.PropertyToID("_FeatureTilesLists");

	private static LocalKeyword s_LinearToSrgbConversion;

	private static int[] s_VariantKernels;

	private static int s_Result = Shader.PropertyToID("_Result");

	private static int s_ResultSizeX = Shader.PropertyToID("_ResultSizeX");

	private static int s_ResultSizeY = Shader.PropertyToID("_ResultSizeY");

	public static bool ShouldApplyLighting(in RecordContext context)
	{
		return context.CameraData.IsLightingEnabled;
	}

	public static void ComputeTilesMinMaxZPass(in RecordContext context)
	{
		RenderGraph renderGraph = context.RenderGraph;
		WaaaghLights lights = context.Lights;
		ComputeShader computeTilesMinMaxZCS = context.Shaders.ComputeTilesMinMaxZCS;
		TilesMinMaxZPassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<TilesMinMaxZPassData>("ComputeTilesMinMaxZPass", out passData, WaaaghProfileId.TilesMinMaxZPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 42);
		passData.Shader = computeTilesMinMaxZCS;
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		passData.MaxDepthOnly = true;
		TextureDesc desc = new TextureDesc((int)lights.ClusteringParams.x, (int)lights.ClusteringParams.y);
		desc.colorFormat = (passData.MaxDepthOnly ? GraphicsFormat.R32_SFloat : GraphicsFormat.R32G32_SFloat);
		desc.enableRandomWrite = true;
		desc.name = "TilesMinMaxZ";
		passData.TilesMinMaxZTexture = renderGraph.CreateTexture(in desc);
		computeRenderGraphBuilder.UseTexture(in passData.TilesMinMaxZTexture, AccessFlags.Write);
		passData.DispatchSize = new int3(desc.width, desc.height, 1);
		passData.TileSize = (int)lights.ClusteringParams.z;
		computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		computeRenderGraphBuilder.SetGlobalTextureAfterPass(in passData.TilesMinMaxZTexture, GlobalTextureShaderPropertyId._TilesMinMaxZTexture);
		computeRenderGraphBuilder.SetRenderFunc<TilesMinMaxZPassData>(ExecuteTilesMinMaxZPass);
	}

	private static void ExecuteTilesMinMaxZPass(TilesMinMaxZPassData data, ComputeGraphContext context)
	{
		context.cmd.DisableShaderKeyword(MINMAXZ_TILE_SIZE_8);
		context.cmd.DisableShaderKeyword(MINMAXZ_TILE_SIZE_16);
		context.cmd.DisableShaderKeyword(MINMAXZ_TILE_SIZE_32);
		switch (data.TileSize)
		{
		case 8:
			context.cmd.EnableShaderKeyword(MINMAXZ_TILE_SIZE_8);
			break;
		case 16:
			context.cmd.EnableShaderKeyword(MINMAXZ_TILE_SIZE_16);
			break;
		case 32:
			context.cmd.EnableShaderKeyword(MINMAXZ_TILE_SIZE_32);
			break;
		}
		if (data.MaxDepthOnly)
		{
			context.cmd.EnableShaderKeyword(MAX_ONLY);
		}
		else
		{
			context.cmd.DisableShaderKeyword(MAX_ONLY);
		}
		context.cmd.SetComputeTextureParam(data.Shader, 0, _TilesMinMaxZUAV, data.TilesMinMaxZTexture);
		context.cmd.DispatchCompute(data.Shader, 0, data.DispatchSize.x, data.DispatchSize.y, data.DispatchSize.z);
	}

	public static void SetupLightDataPass(in RecordContext context)
	{
		SetupLightDataPassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = context.RenderGraph.AddComputePass<SetupLightDataPassData>("SetupLightDataPass", out passData, WaaaghProfileId.SetupLightDataPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 112);
		passData.LightDataConstantBufferHandle = context.FrameResources.DeferredLightingResources.LightDataConstantBuffer;
		computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.LightDataConstantBuffer, AccessFlags.Write);
		passData.LightVolumeDataConstantBufferHandle = context.FrameResources.DeferredLightingResources.LightVolumeDataConstantBuffer;
		computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.LightVolumeDataConstantBuffer, AccessFlags.Write);
		passData.LightDataRaw = context.Lights.LightDataRaw;
		passData.LightVolumeDataRaw = context.Lights.LightVolumeDataRaw;
		passData.ClusteringParams = context.Lights.ClusteringParams;
		passData.LightDataParams = context.Lights.LightDataParams;
		computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		computeRenderGraphBuilder.SetRenderFunc<SetupLightDataPassData>(ExecuteSetupLightDataPass);
	}

	private static void ExecuteSetupLightDataPass(SetupLightDataPassData data, ComputeGraphContext context)
	{
		context.cmd.SetBufferData(data.LightDataConstantBufferHandle, data.LightDataRaw);
		context.cmd.SetBufferData(data.LightVolumeDataConstantBufferHandle, data.LightVolumeDataRaw);
		context.cmd.SetGlobalConstantBuffer(data.LightDataConstantBufferHandle, ShaderPropertyId.LightDataConstantBuffer, 0, data.LightDataRaw.Length * 4 * 4);
		context.cmd.SetGlobalConstantBuffer(data.LightVolumeDataConstantBufferHandle, ShaderPropertyId.LightVolumeDataCB, 0, data.LightVolumeDataRaw.Length * 4 * 4);
		context.cmd.SetGlobalVector(ShaderPropertyId._LightDataParams, data.LightDataParams);
		context.cmd.SetGlobalVector(ShaderPropertyId._ClusteringParams, data.ClusteringParams);
	}

	public static void LightCullingPass(in RecordContext context)
	{
		LightCullingPassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = context.RenderGraph.AddComputePass<LightCullingPassData>("LightCullingPass", out passData, WaaaghProfileId.LightCullingPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 157);
		Vector4 clusteringParams = context.Lights.ClusteringParams;
		Vector4 lightDataParams = context.Lights.LightDataParams;
		int x = (int)(clusteringParams.x * clusteringParams.y);
		int x2 = (int)lightDataParams.z;
		passData.LightCullingShader = context.Shaders.LightCullingShader;
		ComputeShaderKernelDescriptor kernelDescriptor = passData.LightCullingShader.GetKernelDescriptor("BuildLightTiles");
		passData.BuildLightTilesKernel = kernelDescriptor.Index;
		passData.DispatchSize = new int3(RenderingUtils.DivRoundUp(x, (int)kernelDescriptor.ThreadGroupSize.x), 1, math.max(1, RenderingUtils.DivRoundUp(x2, 32)));
		passData.LightTilesBuffer = context.FrameResources.DeferredLightingResources.LightTilesBuffer;
		computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.LightTilesBuffer, AccessFlags.Write);
		computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.LightVolumeDataConstantBuffer);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._TilesMinMaxZTexture);
		passData.ScreenProjMatrix = GetScreenProjMatrix(context.CameraData);
		passData.LightTilesBufferSize = context.Lights.LightTilesBuffer.count;
		computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		computeRenderGraphBuilder.SetRenderFunc<LightCullingPassData>(ExecuteLightCullingPass);
	}

	private static Matrix4x4 GetScreenProjMatrix(WaaaghCameraData cameraData)
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

	private static void ExecuteLightCullingPass(LightCullingPassData data, ComputeGraphContext context)
	{
		context.cmd.SetComputeMatrixParam(data.LightCullingShader, ShaderPropertyId._ScreenProjMatrix, data.ScreenProjMatrix);
		context.cmd.SetComputeBufferParam(data.LightCullingShader, data.BuildLightTilesKernel, ShaderPropertyId._LightTilesBufferUAV, data.LightTilesBuffer);
		context.cmd.SetComputeIntParam(data.LightCullingShader, ShaderPropertyId._LightTilesBufferUAVSize, data.LightTilesBufferSize);
		context.cmd.DispatchCompute(data.LightCullingShader, data.BuildLightTilesKernel, data.DispatchSize.x, data.DispatchSize.y, data.DispatchSize.z);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._LightTilesBuffer, data.LightTilesBuffer);
	}

	public static void LightingPassRaster(in RecordContext context)
	{
		LightingPassRasterData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<LightingPassRasterData>("LightingPassRaster", out passData, WaaaghProfileId.DeferredLightingPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 227);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth, AccessFlags.Read);
		passData.Material = context.MaterialLibrary.DeferredLightingMaterial;
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraAlbedoRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraSpecularRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraBakedGIRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraShadowmaskRT);
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		Color glossyEnvironmentColor = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
		passData.GlossyEnvironmentColor = glossyEnvironmentColor;
		passData.GlossyBlackColor = default(Color);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._ShadowmapRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._ScreenSpaceOcclusionTexture);
		rasterRenderGraphBuilder.SetRenderFunc<LightingPassRasterData>(ExecuteLightingPassRaster);
	}

	private static void ExecuteLightingPassRaster(LightingPassRasterData data, RasterGraphContext context)
	{
		context.cmd.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, data.GlossyBlackColor);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
		context.cmd.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, data.GlossyEnvironmentColor);
	}

	internal static void BuildVariantsPass(in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		DeferredLightingResources deferredLightingResources = context.FrameResources.DeferredLightingResources;
		BuildVariantsPassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = context.RenderGraph.AddComputePass<BuildVariantsPassData>("BuildVariantsPass", out passData, WaaaghProfileId.DeferredLightingBuildVariantsPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 302);
		passData.BuildFeatureTilesShader = context.Shaders.DeferredLightingBuildFeatureTilesCS;
		passData.ClearIndirectArgsShader = context.Shaders.DeferredLightingBuildFeatureTilesListsCS;
		passData.BuildFeatureVariantsShader = context.Shaders.DeferredLightingBuildFeatureTilesListsCS;
		passData.BuildFeatureTilesKernel = passData.BuildFeatureTilesShader.FindKernel("BuildFeatureTiles");
		passData.ClearIndirectArgsKernel = passData.ClearIndirectArgsShader.FindKernel("ClearIndirectArgs");
		passData.BuildFeatureVariantsKernel = passData.BuildFeatureVariantsShader.FindKernel("BuildFeatureVariants");
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		passData.FeatureTilesBuffer = deferredLightingResources.FeatureTilesBuffer;
		computeRenderGraphBuilder.UseBuffer(in deferredLightingResources.FeatureTilesBuffer, AccessFlags.Write);
		passData.FeatureTilesListsBuffer = deferredLightingResources.FeatureTilesListsBuffer;
		computeRenderGraphBuilder.UseBuffer(in deferredLightingResources.FeatureTilesListsBuffer, AccessFlags.Write);
		passData.IndirectArgsBuffer = deferredLightingResources.FeatureIndirectArgsBuffer;
		computeRenderGraphBuilder.UseBuffer(in deferredLightingResources.FeatureIndirectArgsBuffer, AccessFlags.Write);
		passData.TilesCountX = Mathf.CeilToInt((float)cameraData.cameraTargetDescriptor.width / 16f);
		passData.TilesCountY = Mathf.CeilToInt((float)cameraData.cameraTargetDescriptor.height / 16f);
		computeRenderGraphBuilder.SetRenderFunc<BuildVariantsPassData>(ExecuteBuildVariantsPass);
	}

	private static void ExecuteBuildVariantsPass(BuildVariantsPassData data, ComputeGraphContext context)
	{
		BuildFeatureTiles(data, context);
		ClearIndirectArgs(data, context);
		BuildFeatureVariants(data, context);
	}

	private static void BuildFeatureTiles(BuildVariantsPassData data, ComputeGraphContext context)
	{
		ComputeShader buildFeatureTilesShader = data.BuildFeatureTilesShader;
		int buildFeatureTilesKernel = data.BuildFeatureTilesKernel;
		context.cmd.SetComputeBufferParam(buildFeatureTilesShader, buildFeatureTilesKernel, s_FeatureTiles, data.FeatureTilesBuffer);
		context.cmd.DispatchCompute(buildFeatureTilesShader, buildFeatureTilesKernel, data.TilesCountX, data.TilesCountY, 1);
	}

	private static void ClearIndirectArgs(BuildVariantsPassData data, ComputeGraphContext context)
	{
		ComputeShader clearIndirectArgsShader = data.ClearIndirectArgsShader;
		int clearIndirectArgsKernel = data.ClearIndirectArgsKernel;
		context.cmd.SetComputeBufferParam(clearIndirectArgsShader, clearIndirectArgsKernel, s_DispatchIndirectArgs, data.IndirectArgsBuffer);
		context.cmd.DispatchCompute(clearIndirectArgsShader, clearIndirectArgsKernel, RenderingUtils.DivRoundUp(8, 32), 1, 1);
	}

	private static void BuildFeatureVariants(BuildVariantsPassData data, ComputeGraphContext context)
	{
		ComputeShader buildFeatureVariantsShader = data.BuildFeatureVariantsShader;
		int buildFeatureVariantsKernel = data.BuildFeatureVariantsKernel;
		int num = data.TilesCountX * data.TilesCountY;
		context.cmd.SetComputeIntParam(buildFeatureVariantsShader, s_FeatureTilesCount, num);
		context.cmd.SetComputeIntParam(buildFeatureVariantsShader, s_FeatureTilesCountX, data.TilesCountX);
		context.cmd.SetComputeBufferParam(buildFeatureVariantsShader, buildFeatureVariantsKernel, s_FeatureTiles, data.FeatureTilesBuffer);
		context.cmd.SetComputeBufferParam(buildFeatureVariantsShader, buildFeatureVariantsKernel, s_DispatchIndirectArgs, data.IndirectArgsBuffer);
		context.cmd.SetComputeBufferParam(buildFeatureVariantsShader, buildFeatureVariantsKernel, s_FeatureTilesLists, data.FeatureTilesListsBuffer);
		context.cmd.DispatchCompute(buildFeatureVariantsShader, buildFeatureVariantsKernel, RenderingUtils.DivRoundUp(num, 32), 1, 1);
	}

	internal static void LightingPassCompute(in RecordContext context)
	{
		ComputeShader shader = (SystemSupportsDxcCompiler() ? context.Shaders.DeferredLightingDxcCS : context.Shaders.DeferredLightingCS);
		EnsureStaticData(shader);
		WaaaghCameraData cameraData = context.CameraData;
		LightingPassComputeData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = context.RenderGraph.AddComputePass<LightingPassComputeData>("LightingPassCompute", out passData, WaaaghProfileId.LightingPassCompute.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 413);
		passData.Shader = shader;
		passData.CameraColorRT = context.FrameResources.CameraStackTargets.Color;
		TextureHandle input = context.FrameResources.CameraStackTargets.Color;
		computeRenderGraphBuilder.UseTexture(in input, AccessFlags.Write);
		input = context.FrameResources.CameraStackTargets.Depth;
		computeRenderGraphBuilder.UseTexture(in input);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraAlbedoRT);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraSpecularRT);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraBakedGIRT);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraShadowmaskRT);
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		Color glossyEnvironmentColor = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
		passData.GlossyEnvironmentColor = glossyEnvironmentColor;
		passData.ShadowQuality = context.ShadowData.ShadowQuality;
		passData.IsCameraColorSrgb = GraphicsFormatUtility.IsSRGBFormat(cameraData.cameraTargetDescriptor.graphicsFormat);
		passData.CameraColorSize.x = cameraData.cameraTargetDescriptor.width;
		passData.CameraColorSize.y = cameraData.cameraTargetDescriptor.height;
		passData.FeatureTilesDimensions = new int2(Mathf.CeilToInt((float)cameraData.cameraTargetDescriptor.width / 16f), Mathf.CeilToInt((float)cameraData.cameraTargetDescriptor.height / 16f));
		passData.FeatureTilesBuffer = computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.FeatureTilesBuffer);
		passData.IndirectArgsBuffer = computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.FeatureIndirectArgsBuffer);
		passData.FeatureTilesListsBuffer = computeRenderGraphBuilder.UseBuffer(in context.FrameResources.DeferredLightingResources.FeatureTilesListsBuffer);
		computeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._ShadowmapRT);
		computeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._ScreenSpaceOcclusionTexture);
		computeRenderGraphBuilder.SetRenderFunc<LightingPassComputeData>(ExecuteLightingPassCompute);
	}

	private static void EnsureStaticData(ComputeShader shader)
	{
		if (s_VariantKernels == null)
		{
			s_LinearToSrgbConversion = new LocalKeyword(shader, "_LINEAR_TO_SRGB_CONVERSION");
			s_VariantKernels = new int[8];
			for (int i = 0; i < 8; i++)
			{
				s_VariantKernels[i] = shader.FindKernel($"Deferred_Indirect_Variant{i}");
			}
		}
	}

	private static void ExecuteLightingPassCompute(LightingPassComputeData data, ComputeGraphContext context)
	{
		ComputeCommandBuffer cmd = context.cmd;
		ComputeShader shader = data.Shader;
		cmd.SetKeyword(shader, in s_LinearToSrgbConversion, data.IsCameraColorSrgb);
		cmd.SetComputeVectorParam(shader, ShaderPropertyId._GlossyEnvironmentColor, Color.clear);
		cmd.SetComputeIntParam(shader, s_ResultSizeX, data.CameraColorSize.x);
		cmd.SetComputeIntParam(shader, s_ResultSizeY, data.CameraColorSize.y);
		cmd.SetComputeIntParam(shader, s_FeatureTilesCount, data.FeatureTilesDimensions.x * data.FeatureTilesDimensions.y);
		cmd.SetComputeIntParam(shader, s_FeatureTilesCountX, data.FeatureTilesDimensions.x);
		for (int i = 0; i < s_VariantKernels.Length; i++)
		{
			int kernelIndex = s_VariantKernels[i];
			cmd.SetComputeTextureParam(shader, kernelIndex, s_Result, data.CameraColorRT);
			cmd.SetComputeBufferParam(shader, kernelIndex, s_FeatureTiles, data.FeatureTilesBuffer);
			cmd.SetComputeBufferParam(shader, kernelIndex, s_FeatureTilesLists, data.FeatureTilesListsBuffer);
			cmd.DispatchCompute(shader, kernelIndex, data.IndirectArgsBuffer, (uint)(i * 12));
		}
		cmd.SetComputeVectorParam(shader, ShaderPropertyId._GlossyEnvironmentColor, data.GlossyEnvironmentColor);
	}

	public static void DeferredFogPass(in RecordContext context)
	{
		DeferredFogPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<DeferredFogPassData>("DeferredFogPass", out passData, WaaaghProfileId.DeferredFogPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredLighting.cs", 514);
		passData.Material = context.MaterialLibrary.DeferredFogMaterial;
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthRT);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(DeferredFogPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
		});
	}

	private static bool SystemSupportsDxcCompiler()
	{
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		if (graphicsDeviceType == GraphicsDeviceType.Direct3D12 || (uint)(graphicsDeviceType - 23) <= 2u)
		{
			return true;
		}
		return false;
	}
}
