using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

internal sealed class VolumetricLightingRendererFeature : IRendererFeature, IDisposable
{
	internal const int kMaxLocalVolumetricFogCount = 512;

	private readonly VolumetricLightingRendererFeatureAsset m_Asset;

	private Material m_ShadowmapDownsampleMaterial;

	private Material m_ApplyOpaqueMaterial;

	private Material m_LocalVolumetricFogDebugMaterial;

	private ComputeShaderKernelDescriptor m_BuilFogTilesKernel;

	private GraphicsBuffer m_VisibleVolumeBoundsBuffer;

	private GraphicsBuffer m_VisibleVolumeDataBuffer;

	private GraphicsBuffer m_FogTilesBuffer;

	private GraphicsBuffer m_ZBinsBuffer;

	private NativeArray<LocalVolumetricFogBounds> m_VisibleVolumeBoundsList;

	private NativeArray<LocalVolumetricFogEngineData> m_VisibleVolumeDataList;

	private NativeArray<int> m_VisibleCounter;

	private NativeArray<LocalFogDescriptor> m_LocalFogDescriptors;

	private NativeArray<ZBin> m_ZBins;

	private Frustum m_Frustum;

	private LocalFogComparer m_LocalFogComparer;

	private Vector4 m_FogClusteringParams;

	private VolumetricLightingFeatureResources m_Resources;

	private RenderRuntimeTextures m_SharedTextures;

	private readonly VolumetricLightingData m_ReusableVolumetricLightingData = new VolumetricLightingData();

	private VolumetricLightingData m_ActiveVolumetricLightingData;

	public VolumetricLightingSettings Settings => m_Asset.Settings;

	public Material ShadowmapDownsampleMaterial => m_ShadowmapDownsampleMaterial;

	public NativeArray<LocalVolumetricFogBounds> VisibleVolumeBoundsList => m_VisibleVolumeBoundsList;

	public NativeArray<LocalVolumetricFogEngineData> VisibleVolumeDataList => m_VisibleVolumeDataList;

	public NativeArray<ZBin> ZBins => m_ZBins;

	public Vector4 FogClusteringParams => m_FogClusteringParams;

	public VolumetricLightingFeatureResources Resources => m_Resources;

	public RenderRuntimeTextures SharedTextures => m_SharedTextures;

	public int VisibleVolumesCount
	{
		get
		{
			if (m_VisibleCounter.IsCreated)
			{
				return m_VisibleCounter[0];
			}
			return 0;
		}
	}

	public VolumetricLightingRendererFeature(VolumetricLightingRendererFeatureAsset asset)
	{
		m_Asset = asset;
		m_Resources = GraphicsSettings.GetRenderPipelineSettings<VolumetricLightingFeatureResources>();
		m_SharedTextures = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeTextures>();
		m_ShadowmapDownsampleMaterial = CoreUtils.CreateEngineMaterial(m_Resources.ShadowmapDownsampleShader);
		m_ApplyOpaqueMaterial = CoreUtils.CreateEngineMaterial(m_Resources.ApplyOpaqueShader);
		m_LocalVolumetricFogDebugMaterial = CoreUtils.CreateEngineMaterial(m_Resources.DebugLocalVolumetricFogPS);
		m_BuilFogTilesKernel = m_Resources.LocalVolumetricFogCullingCS.GetKernelDescriptor("BuildFogTiles");
		m_VisibleVolumeBoundsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 512, Marshal.SizeOf<LocalVolumetricFogBounds>());
		m_VisibleVolumeBoundsBuffer.name = "VisibleLocalVolumetricFogBoundsBuffer";
		m_VisibleVolumeDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 512, Marshal.SizeOf<LocalVolumetricFogEngineData>());
		m_VisibleVolumeDataBuffer.name = "VisibleLocalVolumetricFogDataBuffer";
		m_ZBinsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1024, Marshal.SizeOf<float4>());
		m_ZBinsBuffer.name = "VisibleZBinsBuffer";
		m_VisibleVolumeBoundsList = new NativeArray<LocalVolumetricFogBounds>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_VisibleVolumeDataList = new NativeArray<LocalVolumetricFogEngineData>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_VisibleCounter = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_LocalFogDescriptors = new NativeArray<LocalFogDescriptor>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_ZBins = new NativeArray<ZBin>(4096, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_ShadowmapDownsampleMaterial);
		CoreUtils.Destroy(m_ApplyOpaqueMaterial);
		CoreUtils.Destroy(m_LocalVolumetricFogDebugMaterial);
		VolumetricCameraBuffers.Cleanup();
		m_VisibleVolumeBoundsBuffer.Release();
		m_VisibleVolumeDataBuffer.Release();
		m_FogTilesBuffer?.Release();
		m_ZBinsBuffer.Release();
		m_VisibleVolumeBoundsList.Dispose();
		m_VisibleVolumeDataList.Dispose();
		m_VisibleCounter.Dispose();
		m_LocalFogDescriptors.Dispose();
		m_ZBins.Dispose();
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddScheduleSetupJobsDelegate(OnScheduleSetupJobs);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterDeferredLighting, OnAfterDeferredLighting);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawTransparent1, OnBeforeDrawTransparent);
		registry.AddCleanupDelegate(OnCleanup);
	}

	private void OnCleanup()
	{
		m_ActiveVolumetricLightingData = null;
	}

	private JobHandle OnScheduleSetupJobs(in SetupContext context, JobHandle dependency)
	{
		VolumetricCameraBuffers.CleanUnused();
		VolumetricFog component = VolumeManager.instance.stack.GetComponent<VolumetricFog>();
		if (!context.CameraData.IsLightingEnabled || !(component != null) || !component.IsActive())
		{
			return dependency;
		}
		m_ActiveVolumetricLightingData = m_ReusableVolumetricLightingData;
		m_ActiveVolumetricLightingData.VolumetricFog = component;
		m_ActiveVolumetricLightingData.TileSize = context.Lights.TileSize;
		ref readonly WaaaghCameraData cameraData = ref context.CameraData;
		ref readonly WaaaghRenderingData renderingData = ref context.RenderingData;
		Camera camera = cameraData.camera;
		Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
		Matrix4x4 viewProjMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(cameraData.GetGPUProjectionMatrix(), viewMatrix, camera.orthographic);
		Frustum.Create(ref m_Frustum, viewProjMatrix, camera.transform.position, camera.transform.forward, camera.nearClipPlane, camera.farClipPlane);
		List<LocalVolumetricFog> volumes = LocalVolumetricFogManager.Instance.Volumes;
		int num = math.min(volumes.Count, 512);
		for (int i = 0; i < num; i++)
		{
			LocalVolumetricFog localVolumetricFog = volumes[i];
			localVolumetricFog.PrepareParameters(renderingData.TimeData.Time);
			m_LocalFogDescriptors[i] = new LocalFogDescriptor
			{
				Data = localVolumetricFog.Parameters.ConvertToEngineData(),
				Position = localVolumetricFog.transform.position,
				Rotation = localVolumetricFog.transform.rotation,
				Size = localVolumetricFog.Parameters.Size,
				IsVisible = false,
				MinZ = float.MaxValue,
				MaxZ = float.MaxValue,
				MeanZ = float.MaxValue
			};
		}
		InitFogTilesBuffer(in cameraData, m_ActiveVolumetricLightingData.TileSize);
		CullingJob jobData = default(CullingJob);
		jobData.TotalVolumesCount = num;
		jobData.CameraFrustum = m_Frustum;
		jobData.FogDescs = m_LocalFogDescriptors;
		jobData.VisibleCounter = m_VisibleCounter;
		jobData.Run();
		Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * camera.worldToCameraMatrix;
		JobHandle dependsOn = dependency;
		MinMaxZJob jobData2 = default(MinMaxZJob);
		jobData2.WorldToViewMatrix = matrix4x;
		jobData2.LocalFogDescs = m_LocalFogDescriptors;
		dependsOn = IJobParallelForExtensions.Schedule(jobData2, num, 32, dependsOn);
		dependsOn = m_LocalFogDescriptors.SortJob(m_LocalFogComparer).Schedule(dependsOn);
		int num2 = m_VisibleCounter[0];
		ZBinningJob jobData3 = default(ZBinningJob);
		jobData3.CameraNearClip = camera.nearClipPlane;
		jobData3.ZBinFactor = m_FogClusteringParams.w;
		jobData3.VisibleCount = num2;
		jobData3.FogDescs = m_LocalFogDescriptors;
		jobData3.ZBins = m_ZBins;
		dependsOn = jobData3.Schedule(num2, dependsOn);
		ExtractLocalFogDataJob jobData4 = default(ExtractLocalFogDataJob);
		jobData4.WorldToViewMatrix = matrix4x;
		jobData4.LocalFogDescriptors = m_LocalFogDescriptors;
		jobData4.LocalFogEngineData = m_VisibleVolumeDataList;
		jobData4.Obbs = m_VisibleVolumeBoundsList;
		return IJobParallelForExtensions.Schedule(jobData4, num2, 32, dependsOn);
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		if (m_ActiveVolumetricLightingData != null)
		{
			m_ActiveVolumetricLightingData.VisibleVolumesBoundsBuffer = context.RenderGraph.ImportBuffer(m_VisibleVolumeBoundsBuffer);
			m_ActiveVolumetricLightingData.VisibleVolumesDataBuffer = context.RenderGraph.ImportBuffer(m_VisibleVolumeDataBuffer);
			m_ActiveVolumetricLightingData.ZBinsBuffer = context.RenderGraph.ImportBuffer(m_ZBinsBuffer);
			m_ActiveVolumetricLightingData.FogTilesBuffer = context.RenderGraph.ImportBuffer(m_FogTilesBuffer);
			m_ActiveVolumetricLightingData.FogTilesBufferCount = m_FogTilesBuffer.count;
		}
	}

	private void OnAfterDeferredLighting(in RecordContext context)
	{
		if (m_ActiveVolumetricLightingData != null)
		{
			if (m_Asset.Settings.LocalVolumesEnabled)
			{
				LocalVolumetricFogCullingPass.Record(in context, this, m_Resources.LocalVolumetricFogCullingCS, m_BuilFogTilesKernel, m_ActiveVolumetricLightingData);
			}
			VolumetricLightingPass.Record(in context, this, m_ShadowmapDownsampleMaterial, m_ActiveVolumetricLightingData);
		}
	}

	private void OnBeforeDrawTransparent(in RecordContext context)
	{
		if (m_ActiveVolumetricLightingData != null)
		{
			VolumetricLightingApplyOpaquePass.Record(in context, m_ApplyOpaqueMaterial, m_ActiveVolumetricLightingData);
			if (m_Asset.Settings.DebugLocalVolumetricFog)
			{
				DebugLocalVolumetricFogPass.Record(in context, this, m_LocalVolumetricFogDebugMaterial, m_ActiveVolumetricLightingData);
			}
		}
	}

	private void InitFogTilesBuffer(in WaaaghCameraData cameraData, TileSize tileSize)
	{
		ref RenderTextureDescriptor cameraTargetDescriptor = ref cameraData.cameraTargetDescriptor;
		int2 @int = 1;
		@int.x = RenderingUtils.DivRoundUp(cameraTargetDescriptor.width, (int)tileSize);
		@int.y = RenderingUtils.DivRoundUp(cameraTargetDescriptor.height, (int)tileSize);
		int num = 16;
		int num2 = @int.x * @int.y * num;
		if (m_FogTilesBuffer == null || !m_FogTilesBuffer.IsValid() || m_FogTilesBuffer.count < num2)
		{
			if (m_FogTilesBuffer != null)
			{
				m_FogTilesBuffer.Release();
			}
			if (num2 > 0)
			{
				m_FogTilesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num2, Marshal.SizeOf<uint>());
				m_FogTilesBuffer.name = "_LightTilesBuffer";
			}
		}
		Camera camera = cameraData.camera;
		float w = 4096f / (camera.farClipPlane - camera.nearClipPlane);
		m_FogClusteringParams = new Vector4(@int.x, @int.y, (float)tileSize, w);
	}
}
