using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

internal static class VolumetricLightingPass
{
	private sealed class PassData
	{
		public Material ShadowmapDownsampleMaterial;

		public ComputeShader VoxelizationShader;

		public ComputeShader LightingShader;

		public ComputeShader ScatterShader;

		public TextureHandle VoxelizedSceneTexture;

		public TextureHandle LightingHistoryTexture;

		public TextureHandle ScatterTexture;

		public Texture2D BlueNoiseTexture;

		public TextureHandle Shadowmap;

		public TextureHandle ShadowmapDownsampled;

		public int3 VoxelizationDispatchSize;

		public int3 LightingDispatchSize;

		public int3 ScatterDispatchSize;

		public Vector4 LightingTextureSize;

		public Matrix4x4 ViewProjMatrix;

		public Matrix4x4 InvViewProjMatrix;

		public Matrix4x4 PrevViewProjMatrix;

		public Vector4 VolumetricProjectionParams;

		public Vector4 HeightFogParams;

		public Vector4 DownsampledShadowmapSize;

		public float BlueNoiseTextureSize;

		public Vector4 ScatteringExtinction;

		public float Anisotropy;

		public bool TricubicDeferred;

		public bool TricubicForward;

		public float LightShadows;

		public float TemporalFeedback;

		public float AmbientLightScale;

		public bool HighRes;

		public bool TemporalAccumulation;

		public bool UseDownsampledShadowmap;

		public bool LocalVolumesEnabled;

		public Vector4 LocalVolumetricFogClusteringParams;

		public BufferHandle LocalFogBoundsBuffer;

		public BufferHandle LocalFogGpuDataBuffer;

		public BufferHandle LocalFogTilesBuffer;

		public BufferHandle LocalFogZBinsBuffer;

		public Matrix4x4 ScreenProjMatrix;

		public Texture VolumeMaskAtlas;

		public bool SkipFirstFrameTemporalAccumulation;
	}

	private static readonly int _ResultUAV = Shader.PropertyToID("_ResultUAV");

	private static readonly int _VoxelizedSceneTexture = Shader.PropertyToID("_VoxelizedSceneTexture");

	private static readonly int _HistoryTexture = Shader.PropertyToID("_HistoryTexture");

	private static readonly int _BlueNoiseTex_Size = Shader.PropertyToID("_BlueNoiseTex_Size");

	private static readonly int _VolumetricViewProj = Shader.PropertyToID("_VolumetricViewProj");

	private static readonly int _VolumetricInvViewProj = Shader.PropertyToID("_VolumetricInvViewProj");

	private static readonly int _VolumetricPrevViewProj = Shader.PropertyToID("_VolumetricPrevViewProj");

	private static readonly int _VolumetricProjectionParams = Shader.PropertyToID("_VolumetricProjectionParams");

	private static readonly int _VolumetricScatteringExtinction = Shader.PropertyToID("_VolumetricScatteringExtinction");

	private static readonly int _VolumetricHeightFogScatteringExtinction = Shader.PropertyToID("_VolumetricHeightFogScatteringExtinction");

	private static readonly int _VolumetricHeightFogParams = Shader.PropertyToID("_VolumetricHeightFogParams");

	private static readonly int _Anisotropy = Shader.PropertyToID("_Anisotropy");

	private static readonly int _VolumeScatter_Size = Shader.PropertyToID("_VolumeScatter_Size");

	private static readonly int _VolumtricTricubicDeferred = Shader.PropertyToID("_VolumtricTricubicDeferred");

	private static readonly int _VolumtricTricubicForward = Shader.PropertyToID("_VolumtricTricubicForward");

	private static readonly int _VolumetricLightShadows = Shader.PropertyToID("_VolumetricLightShadows");

	private static readonly int _TemporalFeedback = Shader.PropertyToID("_TemporalFeedback");

	private static readonly int _VolumetricAmbientLightScale = Shader.PropertyToID("_VolumetricAmbientLightScale");

	private static readonly int _OutputSize = Shader.PropertyToID("_OutputSize");

	private static readonly int _VolumeInject = Shader.PropertyToID("_VolumeInject");

	private static readonly int _BlueNoiseTex = Shader.PropertyToID("_BlueNoiseTex");

	private static readonly int _VolumeMaskAtlas = Shader.PropertyToID("_VolumeMaskAtlas");

	private static readonly int _SourceTexture = Shader.PropertyToID("_SourceTexture");

	private static readonly int _VolumetricLightingShadowmapRT = Shader.PropertyToID("_VolumetricLightingShadowmapRT");

	private static readonly string TEMPORAL_ACCUMULATION = "TEMPORAL_ACCUMULATION";

	private static readonly string _LOCAL_VOLUMETRIC_VOLUMES_ENABLED = "_LOCAL_VOLUMETRIC_VOLUMES_ENABLED";

	private static readonly int3 m_VoxelizationGroupSize = new int3(8, 8, 8);

	private static readonly int3 m_LightingGroupSize = new int3(8, 8, 8);

	private static readonly int3 m_ScatterGroupSize = new int3(8, 8, 1);

	public static void Record(in RecordContext context, VolumetricLightingRendererFeature feature, Material shadowDownsampleMaterial, VolumetricLightingData volumetricLightingData)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("VolumetricLighting", out passData, WaaaghProfileId.VolumetricLighting.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\VolumetricLighting\\Passes\\VolumetricLightingPass.cs", 105);
		passData.ShadowmapDownsampleMaterial = shadowDownsampleMaterial;
		passData.VoxelizationShader = feature.Resources.VoxelizationShader;
		passData.LightingShader = feature.Resources.LightingShader;
		passData.ScatterShader = feature.Resources.ScatterShader;
		Vector2Int cameraRenderPixelSize = new Vector2Int(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		int3 textureSize = new int3(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height, (int)feature.Settings.Slices);
		textureSize.x = RenderingUtils.DivRoundUp(textureSize.x, (int)volumetricLightingData.TileSize);
		textureSize.y = RenderingUtils.DivRoundUp(textureSize.y, (int)volumetricLightingData.TileSize);
		VolumetricCameraBuffer volumetricCameraBuffer = VolumetricCameraBuffers.EnsureCamera(context.CameraData.camera, cameraRenderPixelSize, textureSize, feature.Settings.TemporalAccumulation ? 1 : 0);
		passData.SkipFirstFrameTemporalAccumulation = !volumetricCameraBuffer.IsFirstFrame;
		TextureDesc desc = new TextureDesc(textureSize.x, textureSize.y);
		desc.slices = textureSize.z;
		desc.dimension = TextureDimension.Tex3D;
		desc.enableRandomWrite = true;
		desc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		desc.name = "VoxelizedScene";
		passData.VoxelizedSceneTexture = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		desc.name = "VolumetricScatter";
		volumetricLightingData.ScatterTexture = context.RenderGraph.CreateTexture(in desc);
		passData.ScatterTexture = volumetricLightingData.ScatterTexture;
		unsafeRenderGraphBuilder.UseTexture(in passData.ScatterTexture, AccessFlags.Write);
		passData.TemporalAccumulation = feature.Settings.TemporalAccumulation;
		passData.TemporalFeedback = feature.Settings.TemporalFeedback;
		if (feature.Settings.TemporalAccumulation)
		{
			passData.LightingHistoryTexture = context.RenderGraph.ImportTexture(volumetricCameraBuffer.GetCurrentFrameRT());
			unsafeRenderGraphBuilder.UseTexture(in passData.LightingHistoryTexture, AccessFlags.ReadWrite);
			passData.BlueNoiseTexture = feature.SharedTextures.BlueNoise16Textures[context.RenderingData.TimeData.FrameId % feature.SharedTextures.BlueNoise16Textures.Length];
		}
		else
		{
			passData.BlueNoiseTexture = feature.SharedTextures.BlueNoise16Textures[0];
		}
		bool num = WaaaghPipeline.Asset.ShadowSettings.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(context.CameraData.maxShadowDistance, 0f);
		int num2;
		if (!num)
		{
			num2 = ((!flag) ? 1 : 0);
			if (num2 != 0)
			{
				passData.Shadowmap = context.FrameResources.Shadows.Shadowmap;
				unsafeRenderGraphBuilder.UseTexture(in context.FrameResources.Shadows.Shadowmap);
				goto IL_02d6;
			}
		}
		else
		{
			num2 = 0;
		}
		passData.Shadowmap = context.RenderGraph.defaultResources.defaultShadowTexture;
		TextureHandle input = context.RenderGraph.defaultResources.defaultShadowTexture;
		unsafeRenderGraphBuilder.UseTexture(in input);
		goto IL_02d6;
		IL_02d6:
		if (num2 != 0 && feature.Settings.UseDownsampledShadowmap && feature.Settings.DownsampledShadowmapSize < WaaaghPipeline.Asset.ShadowSettings.AtlasSize)
		{
			passData.UseDownsampledShadowmap = true;
			int downsampledShadowmapSize = (int)feature.Settings.DownsampledShadowmapSize;
			TextureDesc desc2 = new TextureDesc(downsampledShadowmapSize, downsampledShadowmapSize);
			desc2.isShadowMap = true;
			desc2.depthBufferBits = DepthBits.Depth16;
			desc2.filterMode = FilterMode.Bilinear;
			desc2.wrapMode = TextureWrapMode.Clamp;
			desc2.dimension = TextureDimension.Tex2D;
			desc2.useMipMap = false;
			desc2.name = "ShadowmapDownsampledRT";
			passData.ShadowmapDownsampled = unsafeRenderGraphBuilder.CreateTransientTexture(in desc2);
			passData.DownsampledShadowmapSize = new Vector4(downsampledShadowmapSize, downsampledShadowmapSize, 1f / (float)downsampledShadowmapSize, 1f / (float)downsampledShadowmapSize);
		}
		else
		{
			passData.UseDownsampledShadowmap = false;
		}
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._TilesMinMaxZTexture);
		Camera camera = context.CameraData.camera;
		float nearClipPlane = camera.nearClipPlane;
		float num3 = math.min(feature.Settings.FarClip, camera.farClipPlane);
		float z = nearClipPlane / num3;
		float w = num3 / nearClipPlane;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 proj = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, nearClipPlane, num3);
		bool flag2 = true;
		bool renderIntoTexture = SystemInfo.graphicsUVStartsAtTop && flag2;
		proj = GL.GetGPUProjectionMatrix(proj, renderIntoTexture);
		Matrix4x4 viewProjMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(proj, worldToCameraMatrix, camera.orthographic);
		Matrix4x4 matrix4x = Matrix4x4.Inverse(worldToCameraMatrix);
		Matrix4x4 matrix4x2 = Matrix4x4.Inverse(proj);
		Matrix4x4 invViewProjMatrix = matrix4x * matrix4x2;
		passData.ViewProjMatrix = viewProjMatrix;
		passData.InvViewProjMatrix = invViewProjMatrix;
		passData.PrevViewProjMatrix = volumetricCameraBuffer.PrevViewProjMatrix;
		passData.VolumetricProjectionParams = new Vector4(nearClipPlane, num3, z, w);
		VolumetricFog volumetricFog = volumetricLightingData.VolumetricFog;
		float num4 = ScaleHeightFromLayerDepth(Mathf.Max(0.01f, volumetricFog.FogHeight.value));
		passData.HeightFogParams = new Vector4(volumetricFog.BaseHeight.value, 1f / num4, num4, volumetricFog.HeightFogEnabled.value ? 1 : 0);
		passData.LightingTextureSize = new Vector4(textureSize.x, textureSize.y, textureSize.z);
		passData.VoxelizationDispatchSize = new int3(RenderingUtils.DivRoundUp(textureSize.x, m_VoxelizationGroupSize.x), RenderingUtils.DivRoundUp(textureSize.y, m_VoxelizationGroupSize.y), RenderingUtils.DivRoundUp(textureSize.z, m_VoxelizationGroupSize.z));
		passData.LightingDispatchSize = new int3(RenderingUtils.DivRoundUp(textureSize.x, m_LightingGroupSize.x), RenderingUtils.DivRoundUp(textureSize.y, m_LightingGroupSize.y), RenderingUtils.DivRoundUp(textureSize.z, m_LightingGroupSize.z));
		passData.ScatterDispatchSize = new int3(RenderingUtils.DivRoundUp(textureSize.x, m_ScatterGroupSize.x), RenderingUtils.DivRoundUp(textureSize.y, m_ScatterGroupSize.y), 1);
		passData.BlueNoiseTextureSize = passData.BlueNoiseTexture.width;
		float num5 = 1f / volumetricFog.FogDistanceAttenuation.value;
		Vector4 scatteringExtinction = (Vector4)CoreUtils.ConvertSRGBToActiveColorSpace(volumetricFog.Albedo.value) * num5;
		passData.ScatteringExtinction = scatteringExtinction;
		passData.ScatteringExtinction.w = num5;
		passData.Anisotropy = volumetricFog.Anisotropy.value;
		passData.TricubicDeferred = feature.Settings.TricubicFilteringDeferred;
		passData.TricubicForward = feature.Settings.TricubicFilteringForward;
		passData.LightShadows = (float)feature.Settings.LightShadows;
		passData.AmbientLightScale = volumetricFog.AmbientLightMultiplier.value;
		passData.HighRes = feature.Settings.Slices == VolumetricLightingSlices.x128;
		passData.LocalVolumesEnabled = feature.Settings.LocalVolumesEnabled;
		if (passData.LocalVolumesEnabled)
		{
			passData.LocalVolumetricFogClusteringParams = feature.FogClusteringParams;
			passData.LocalFogBoundsBuffer = volumetricLightingData.VisibleVolumesBoundsBuffer;
			passData.LocalFogTilesBuffer = volumetricLightingData.FogTilesBuffer;
			passData.LocalFogGpuDataBuffer = volumetricLightingData.VisibleVolumesDataBuffer;
			passData.LocalFogZBinsBuffer = volumetricLightingData.ZBinsBuffer;
			unsafeRenderGraphBuilder.UseBuffer(in passData.LocalFogBoundsBuffer);
			unsafeRenderGraphBuilder.UseBuffer(in passData.LocalFogTilesBuffer);
			unsafeRenderGraphBuilder.UseBuffer(in passData.LocalFogGpuDataBuffer);
			unsafeRenderGraphBuilder.UseBuffer(in passData.LocalFogZBinsBuffer);
			passData.ScreenProjMatrix = GetScreenProjMatrix(in context.CameraData);
			passData.VolumeMaskAtlas = LocalVolumetricFogManager.Instance.VolumeAtlas.GetAtlas();
			if (passData.VolumeMaskAtlas == null)
			{
				passData.VolumeMaskAtlas = CoreUtils.blackVolumeTexture;
			}
		}
		volumetricCameraBuffer.Swap(passData.ViewProjMatrix);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			Render(data, context);
		});
	}

	private static void Render(PassData data, UnsafeGraphContext context)
	{
		TextureHandle rt;
		if (data.UseDownsampledShadowmap)
		{
			context.cmd.SetRenderTarget(data.ShadowmapDownsampled);
			context.cmd.SetGlobalTexture(_SourceTexture, data.Shadowmap);
			context.cmd.SetGlobalVector(_OutputSize, data.DownsampledShadowmapSize);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ShadowmapDownsampleMaterial, 0, MeshTopology.Triangles, 3);
			rt = data.ShadowmapDownsampled;
		}
		else
		{
			rt = data.Shadowmap;
		}
		context.cmd.SetComputeTextureParam(data.VoxelizationShader, 0, _ResultUAV, data.VoxelizedSceneTexture);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumetricScatteringExtinction, data.ScatteringExtinction);
		context.cmd.SetComputeMatrixParam(data.VoxelizationShader, _VolumetricInvViewProj, data.InvViewProjMatrix);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumetricHeightFogParams, data.HeightFogParams);
		if (data.LocalVolumesEnabled)
		{
			context.cmd.SetComputeVectorParam(data.VoxelizationShader, ShaderPropertyId._LocalVolumetricFogClusteringParams, data.LocalVolumetricFogClusteringParams);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._VisibleVolumeBoundsBuffer, data.LocalFogBoundsBuffer);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._VisibleVolumeDataBuffer, data.LocalFogGpuDataBuffer);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._FogTilesBuffer, data.LocalFogTilesBuffer);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._LocalFogZBinsBuffer, data.LocalFogZBinsBuffer);
			context.cmd.SetComputeMatrixParam(data.VoxelizationShader, ShaderPropertyId._ScreenProjMatrix, data.ScreenProjMatrix);
			context.cmd.SetComputeTextureParam(data.VoxelizationShader, 0, _VolumeMaskAtlas, data.VolumeMaskAtlas);
			context.cmd.EnableShaderKeyword(_LOCAL_VOLUMETRIC_VOLUMES_ENABLED);
		}
		else
		{
			context.cmd.DisableShaderKeyword(_LOCAL_VOLUMETRIC_VOLUMES_ENABLED);
		}
		context.cmd.DispatchCompute(data.VoxelizationShader, 0, data.VoxelizationDispatchSize.x, data.VoxelizationDispatchSize.y, data.VoxelizationDispatchSize.z);
		if (data.TemporalAccumulation && data.SkipFirstFrameTemporalAccumulation)
		{
			context.cmd.EnableShaderKeyword(TEMPORAL_ACCUMULATION);
		}
		else
		{
			context.cmd.DisableShaderKeyword(TEMPORAL_ACCUMULATION);
		}
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _ResultUAV, data.ScatterTexture);
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _VoxelizedSceneTexture, data.VoxelizedSceneTexture);
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _VolumetricLightingShadowmapRT, rt);
		if (data.TemporalAccumulation)
		{
			context.cmd.SetComputeTextureParam(data.LightingShader, 0, _HistoryTexture, data.LightingHistoryTexture);
		}
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _BlueNoiseTex, data.BlueNoiseTexture);
		context.cmd.SetComputeVectorParam(data.LightingShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.SetComputeMatrixParam(data.LightingShader, _VolumetricViewProj, data.ViewProjMatrix);
		context.cmd.SetComputeMatrixParam(data.LightingShader, _VolumetricInvViewProj, data.InvViewProjMatrix);
		context.cmd.SetComputeMatrixParam(data.LightingShader, _VolumetricPrevViewProj, data.PrevViewProjMatrix);
		context.cmd.SetComputeVectorParam(data.LightingShader, _VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetComputeFloatParam(data.LightingShader, _BlueNoiseTex_Size, data.BlueNoiseTextureSize);
		context.cmd.SetComputeFloatParam(data.LightingShader, _Anisotropy, data.Anisotropy);
		context.cmd.SetComputeFloatParam(data.LightingShader, _VolumetricLightShadows, data.LightShadows);
		context.cmd.SetComputeFloatParam(data.LightingShader, _TemporalFeedback, data.TemporalFeedback);
		context.cmd.SetComputeFloatParam(data.LightingShader, _VolumetricAmbientLightScale, data.AmbientLightScale);
		context.cmd.SetComputeVectorParam(data.LightingShader, _VolumetricScatteringExtinction, data.ScatteringExtinction);
		RenderingUtils.SetLightProbe(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), RenderSettings.ambientProbe);
		context.cmd.DispatchCompute(data.LightingShader, 0, data.LightingDispatchSize.x, data.LightingDispatchSize.y, data.LightingDispatchSize.z);
		TextureHandle rt2 = ((!data.TemporalAccumulation) ? data.VoxelizedSceneTexture : data.LightingHistoryTexture);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 1, _VolumeInject, data.ScatterTexture);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 1, _ResultUAV, rt2);
		context.cmd.SetComputeVectorParam(data.ScatterShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.DispatchCompute(data.ScatterShader, 1, data.LightingDispatchSize.x, data.LightingDispatchSize.y, data.LightingDispatchSize.z);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 0, _VolumeInject, rt2);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 0, _ResultUAV, data.ScatterTexture);
		context.cmd.SetComputeVectorParam(data.ScatterShader, _VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetComputeVectorParam(data.ScatterShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.DispatchCompute(data.ScatterShader, 0, data.ScatterDispatchSize.x, data.ScatterDispatchSize.y, data.ScatterDispatchSize.z);
		context.cmd.SetGlobalTexture(ShaderPropertyId._VolumeScatter, data.ScatterTexture);
		context.cmd.SetGlobalFloat(ShaderPropertyId._VolumetricLightingEnabled, 1f);
		context.cmd.SetGlobalVector(_VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.SetGlobalFloat(_VolumtricTricubicDeferred, data.TricubicDeferred ? 1 : 0);
		context.cmd.SetGlobalFloat(_VolumtricTricubicForward, data.TricubicForward ? 1 : 0);
		context.cmd.SetGlobalVector(_VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetGlobalMatrix(_VolumetricViewProj, data.ViewProjMatrix);
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

	private static float ScaleHeightFromLayerDepth(float d)
	{
		return d * 0.144765f;
	}
}
