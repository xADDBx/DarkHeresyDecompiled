using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

public class WaaaghLights
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct LightDescSorter : IComparer<LightDescriptor>
	{
		public int Compare(LightDescriptor x, LightDescriptor y)
		{
			if (x.MeanZ > y.MeanZ)
			{
				return 1;
			}
			if (x.MeanZ < y.MeanZ)
			{
				return -1;
			}
			return 0;
		}
	}

	public const int kMaxVisibleLights = 1024;

	public const int kMaxZBinsCount = 4096;

	private readonly ScriptableRenderer m_ScriptableRenderer;

	private GraphicsBuffer m_LightDataConstantBuffer;

	private GraphicsBuffer m_LightVolumeDataConstantBuffer;

	private GraphicsBuffer m_ZBinsConstantBuffer;

	private GraphicsBuffer m_LightTilesBuffer;

	private GraphicsBuffer m_DeferredLightingFeatureTilesBuffer;

	private GraphicsBuffer m_DeferredLightingIndirectArgsBuffer;

	private GraphicsBuffer m_DeferredLightingFeatureTilesListsBuffer;

	private NativeArray<float4> m_LightDataRaw;

	private NativeArray<float4> m_LightVolumeDataRaw;

	private int m_LightCountClamped;

	private NativeArray<LightDescriptor> m_LightDescs;

	private NativeArray<ZBin> m_ZBins;

	private JobHandle m_SetupJobsHandle;

	private Vector4 m_ClusteringParams;

	private Vector4 m_LightDataParams;

	private Matrix4x4 m_ViewMatrix;

	public bool ShadowmaskEnabled { get; private set; }

	public GraphicsBuffer LightDataConstantBuffer => m_LightDataConstantBuffer;

	public GraphicsBuffer LightVolumeDataConstantBuffer => m_LightVolumeDataConstantBuffer;

	public GraphicsBuffer ZBinsConstantBuffer => m_ZBinsConstantBuffer;

	public GraphicsBuffer LightTilesBuffer => m_LightTilesBuffer;

	public GraphicsBuffer DeferredLightingFeatureTilesBuffer => m_DeferredLightingFeatureTilesBuffer;

	public GraphicsBuffer DeferredLightingIndirectArgsBuffer => m_DeferredLightingIndirectArgsBuffer;

	public GraphicsBuffer DeferredLightingFeatureTilesListsBuffer => m_DeferredLightingFeatureTilesListsBuffer;

	public NativeArray<float4> LightDataRaw => m_LightDataRaw;

	public NativeArray<float4> LightVolumeDataRaw => m_LightVolumeDataRaw;

	public NativeArray<ZBin> ZBins => m_ZBins;

	public Vector4 ClusteringParams => m_ClusteringParams;

	public Vector4 LightDataParams => m_LightDataParams;

	public WaaaghLights(ScriptableRenderer scriptableRenderer)
	{
		m_ScriptableRenderer = scriptableRenderer;
		m_LightDataConstantBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 4096, Marshal.SizeOf<float4>());
		m_LightDataConstantBuffer.name = "LightDataCB";
		m_LightVolumeDataConstantBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 3072, Marshal.SizeOf<float4>());
		m_LightVolumeDataConstantBuffer.name = "LightVolumeDataCB";
		m_ZBinsConstantBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 1024, Marshal.SizeOf<float4>());
		m_ZBinsConstantBuffer.name = "ZBinsCB";
		m_LightDataRaw = new NativeArray<float4>(4096, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_LightVolumeDataRaw = new NativeArray<float4>(3072, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_ZBins = new NativeArray<ZBin>(4096, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public void StartSetupJobs(ScriptableRenderContext context, ContextContainer frameData, TileSize tileSize)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghShadowData waaaghShadowData = frameData.Get<WaaaghShadowData>();
		Camera camera = waaaghCameraData.camera;
		ref NativeArray<VisibleLight> visibleLights = ref waaaghRenderingData.VisibleLights;
		int length = visibleLights.Length;
		m_LightCountClamped = math.min(length, 1024);
		m_LightDescs = new NativeArray<LightDescriptor>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		ShadowmaskEnabled = InitializeLightDescriptors(frameData);
		InitLightTilesBuffer(frameData, tileSize);
		if (ShadowmaskEnabled)
		{
			waaaghRenderingData.PerObjectData |= PerObjectData.ShadowMask;
			waaaghRenderingData.PerObjectData |= PerObjectData.OcclusionProbe;
		}
		SetupDeferredLightingResources(waaaghCameraData);
		int num = 0;
		for (int i = 0; i < length && visibleLights[i].lightType == LightType.Directional; i++)
		{
			num++;
		}
		m_LightDataParams = new Vector4(num, m_LightCountClamped - num, m_LightCountClamped);
		m_ViewMatrix = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * camera.worldToCameraMatrix;
		float w = m_ClusteringParams.w;
		MinMaxZJob minMaxZJob = default(MinMaxZJob);
		minMaxZJob.WorldToViewMatrix = m_ViewMatrix;
		minMaxZJob.LightDescriptors = m_LightDescs;
		MinMaxZJob jobData = minMaxZJob;
		m_SetupJobsHandle = IJobForExtensions.ScheduleParallel(jobData, length, 32, default(JobHandle));
		m_SetupJobsHandle = m_LightDescs.SortJob(default(LightDescSorter)).Schedule(m_SetupJobsHandle);
		m_SetupJobsHandle = waaaghShadowData.ShadowManager.ScheduleSetupJobs(ref m_LightDescs, frameData, m_SetupJobsHandle);
		m_SetupJobsHandle = waaaghRenderingData.LightCookieManager.ScheduleSetupJobs(ref m_LightDescs, m_SetupJobsHandle);
		ZBinningJob zBinningJob = default(ZBinningJob);
		zBinningJob.ZBinFactor = w;
		zBinningJob.CameraNearClip = camera.nearClipPlane;
		zBinningJob.LightCount = length;
		zBinningJob.DirectionalLightCount = num;
		zBinningJob.Lights = m_LightDescs;
		zBinningJob.ZBins = m_ZBins;
		ZBinningJob jobData2 = zBinningJob;
		m_SetupJobsHandle = IJobForExtensions.ScheduleParallel(jobData2, 64, 1, m_SetupJobsHandle);
	}

	private void InitLightTilesBuffer(ContextContainer frameData, TileSize tileSize)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		ref RenderTextureDescriptor cameraTargetDescriptor = ref waaaghCameraData.cameraTargetDescriptor;
		int2 @int = 1;
		@int.x = RenderingUtils.DivRoundUp(cameraTargetDescriptor.width, (int)tileSize);
		@int.y = RenderingUtils.DivRoundUp(cameraTargetDescriptor.height, (int)tileSize);
		int num = 32;
		int num2 = @int.x * @int.y * num;
		if (m_LightTilesBuffer == null || !m_LightTilesBuffer.IsValid() || m_LightTilesBuffer.count < num2)
		{
			if (m_LightTilesBuffer != null)
			{
				m_LightTilesBuffer.Release();
			}
			if (num2 > 0)
			{
				m_LightTilesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num2, Marshal.SizeOf<uint>());
				m_LightTilesBuffer.name = "_LightTilesBuffer";
			}
		}
		Camera camera = waaaghCameraData.camera;
		float w = 4096f / (camera.farClipPlane - camera.nearClipPlane);
		m_ClusteringParams = new Vector4(@int.x, @int.y, (float)tileSize, w);
	}

	public void CompleteSetupJobs(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghShadowData waaaghShadowData = frameData.Get<WaaaghShadowData>();
		m_SetupJobsHandle.Complete();
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.LightCookieSetup)))
		{
			waaaghRenderingData.LightCookieManager.FinishSetup(m_ScriptableRenderer, ref m_LightDescs, frameData);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.ShadowsSetup)))
		{
			waaaghShadowData.ShadowManager.FinishSetup(context, frameData);
		}
		ExtractLightDataJob extractLightDataJob = default(ExtractLightDataJob);
		extractLightDataJob.WorldToViewMatrix = m_ViewMatrix;
		extractLightDataJob.LightDescriptors = m_LightDescs;
		extractLightDataJob.LightData = m_LightDataRaw;
		extractLightDataJob.LightVolumeData = m_LightVolumeDataRaw;
		ExtractLightDataJob jobData = extractLightDataJob;
		m_SetupJobsHandle = IJobForExtensions.ScheduleParallel(jobData, m_LightCountClamped, 32, default(JobHandle));
		m_SetupJobsHandle = m_LightDescs.Dispose(m_SetupJobsHandle);
		m_SetupJobsHandle.Complete();
	}

	private bool InitializeLightDescriptors(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghShadowData waaaghShadowData = frameData.Get<WaaaghShadowData>();
		Span<VisibleLight> span = waaaghRenderingData.VisibleLights.AsSpan();
		int length = span.Length;
		bool flag = false;
		LightDescriptor value;
		for (int i = 0; i < length; m_LightDescs[i] = value, i++)
		{
			ref VisibleLight reference = ref span[i];
			VisibleLight visibleLight = reference;
			Light light = visibleLight.light;
			value = default(LightDescriptor);
			value.VisibleLight = reference;
			value.LightUnsortedIndex = i;
			value.ShadowsCanBeCached = false;
			value.LightLayerMask = 255u;
			LightBakingOutput bakingOutput;
			OwlcatAdditionalLightData component;
			if (light != null)
			{
				value.LightID = light.GetInstanceID();
				bakingOutput = light.bakingOutput;
				if (light.TryGetComponent<OwlcatAdditionalLightData>(out component))
				{
					value.SnapSpecularToInnerRadius = component.SnapSperularToInnerRadius;
					value.LightFalloffType = component.FalloffType;
					value.InnerRadius = component.InnerRadius;
					value.LightLayerMask = (uint)component.LightLayerMask;
					value.VolumetricLighting = component.VolumetricLighting;
					value.VolumetricShadows = component.VolumetricShadows;
					value.VolumetricIntensity = component.VolumetricIntensity;
					value.ShadowmapResolution = component.ShadowmapResolution;
					value.ShadowmapUpdateMode = component.ShadowmapUpdateMode;
					value.ShadowmapAlwaysDrawDynamicShadowCasters = component.ShadowmapAlwaysDrawDynamicShadowCasters;
					value.ShadowmapUpdateOnLightMovement = component.ShadowmapUpdateOnLightMovement;
					if (waaaghShadowData.StaticShadowsCacheEnabled)
					{
						visibleLight = reference;
						if (visibleLight.lightType != LightType.Point)
						{
							visibleLight = reference;
							if (visibleLight.lightType != 0)
							{
								goto IL_01ba;
							}
						}
						value.ShadowsCanBeCached = bakingOutput.lightmapBakeType == LightmapBakeType.Realtime || !bakingOutput.isBaked || bakingOutput.lightmapBakeType == LightmapBakeType.Mixed;
						value.ShadowsCanBeCached = value.ShadowsCanBeCached && component.ShadowmapUpdateMode == ShadowmapUpdateMode.Cached;
					}
					goto IL_01ba;
				}
				value.VolumetricLighting = false;
				value.ColoredShadows = true;
				goto IL_01e8;
			}
			value.ShadowStrength = 1f;
			value.InnerSpotAngle = -1f;
			value.ShadowDataIndex = -1;
			value.ShadowDataIndex = -1;
			value.Shadows = LightShadows.None;
			value.ShadowmapResolution = LightShadowmapResolution.Default;
			value.ShadowmapUpdateMode = ShadowmapUpdateMode.EveryFrame;
			value.ShadowmapAlwaysDrawDynamicShadowCasters = true;
			value.ShadowmapUpdateOnLightMovement = true;
			value.ShadowDepthBias = waaaghShadowData.DepthBias;
			value.ShadowNormalBias = waaaghShadowData.NormalBias;
			value.LightCookieIndex = -1;
			value.lightCookieDescriptor = default(LightCookieDescriptor);
			continue;
			IL_0245:
			value.ShadowNearPlane = light.shadowNearPlane;
			if (component != null && !component.UsePipelineSettings)
			{
				value.ShadowDepthBias = light.shadowBias;
				value.ShadowSlopeBias = component.ShadowSlopeBias;
				value.ShadowNormalBias = light.shadowNormalBias;
			}
			else
			{
				value.ShadowDepthBias = waaaghShadowData.DepthBias;
				value.ShadowSlopeBias = light.type switch
				{
					LightType.Spot => waaaghShadowData.DirectionalSlopeBias, 
					LightType.Directional => waaaghShadowData.DirectionalSlopeBias, 
					LightType.Point => waaaghShadowData.PointSlopeBias, 
					_ => 0f, 
				};
				value.ShadowNormalBias = waaaghShadowData.NormalBias;
			}
			value.InnerSpotAngle = light.innerSpotAngle;
			value.IsBaked = bakingOutput.lightmapBakeType == LightmapBakeType.Baked;
			bool flag2 = RenderingUtils.IsBakedShadowMaskLight(in bakingOutput);
			if (flag2)
			{
				value.ShadowmaskChannel = bakingOutput.occlusionMaskChannel;
			}
			else
			{
				value.ShadowmaskChannel = -1;
			}
			flag = flag || flag2;
			value.ShadowStrength = light.shadowStrength;
			value.ShadowDataIndex = -1;
			value.LightCookieIndex = -1;
			Texture cookie = light.cookie;
			if (cookie == null || component == null)
			{
				value.lightCookieDescriptor = default(LightCookieDescriptor);
				continue;
			}
			value.lightCookieDescriptor.textureId = cookie.GetInstanceID();
			value.lightCookieDescriptor.textureSize = new int2(cookie.width, cookie.height);
			value.lightCookieDescriptor.textureVersion = cookie.updateCount;
			value.lightCookieDescriptor.textureDimension = cookie.dimension;
			value.lightCookieDescriptor.uvSize = component.LightCookieSize;
			value.lightCookieDescriptor.uvOffset = component.LightCookieOffset;
			continue;
			IL_01e8:
			value.Shadows = light.shadows;
			if (value.Shadows != 0)
			{
				if (!waaaghRenderingData.CullResults.GetShadowCasterBounds(i, out var _))
				{
					value.Shadows = LightShadows.None;
				}
				visibleLight = reference;
				if (visibleLight.lightType != 0)
				{
					visibleLight = reference;
					if (visibleLight.lightType != LightType.Point)
					{
						goto IL_0245;
					}
				}
				light.useViewFrustumForShadowCasterCull = false;
			}
			goto IL_0245;
			IL_01ba:
			value.ShadowUpdateFrequencyByDistance = component.ShadowUpdateFrequencyByDistance;
			value.ColoredShadows = component.ColoredShadows;
			goto IL_01e8;
		}
		return flag;
	}

	public void Dispose()
	{
		if (m_LightDataRaw.IsCreated)
		{
			m_LightDataRaw.Dispose();
		}
		if (m_LightVolumeDataRaw.IsCreated)
		{
			m_LightVolumeDataRaw.Dispose();
		}
		if (m_LightDescs.IsCreated)
		{
			m_LightDescs.Dispose();
		}
		if (m_ZBins.IsCreated)
		{
			m_ZBins.Dispose();
		}
		ReleaseBuffers();
	}

	private void ReleaseBuffers()
	{
		if (m_LightDataConstantBuffer != null)
		{
			m_LightDataConstantBuffer.Release();
		}
		if (m_LightVolumeDataConstantBuffer != null)
		{
			m_LightVolumeDataConstantBuffer.Release();
		}
		if (m_ZBinsConstantBuffer != null)
		{
			m_ZBinsConstantBuffer.Release();
		}
		if (m_LightTilesBuffer != null)
		{
			m_LightTilesBuffer.Release();
		}
		if (m_DeferredLightingFeatureTilesBuffer != null && m_DeferredLightingFeatureTilesBuffer.IsValid())
		{
			m_DeferredLightingFeatureTilesBuffer.Release();
		}
		if (m_DeferredLightingIndirectArgsBuffer != null && m_DeferredLightingIndirectArgsBuffer.IsValid())
		{
			m_DeferredLightingIndirectArgsBuffer.Release();
		}
		if (m_DeferredLightingFeatureTilesListsBuffer != null && m_DeferredLightingFeatureTilesListsBuffer.IsValid())
		{
			m_DeferredLightingFeatureTilesListsBuffer.Release();
		}
	}

	private void SetupDeferredLightingResources(WaaaghCameraData cameraData)
	{
		ref RenderTextureDescriptor cameraTargetDescriptor = ref cameraData.cameraTargetDescriptor;
		int2 @int = new int2(cameraTargetDescriptor.width, cameraTargetDescriptor.height) + new int2(15) / new int2(16);
		int num = @int.x * @int.y;
		RenderingUtils.EnsureGraphicsBufferAllocated(ref m_DeferredLightingFeatureTilesBuffer, GraphicsBuffer.Target.Structured, num, 4, "DeferredLightingFeatureTiles");
		RenderingUtils.EnsureGraphicsBufferAllocated(ref m_DeferredLightingIndirectArgsBuffer, GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.IndirectArguments, 8, 12, "DeferredLightingIndirectArgs");
		RenderingUtils.EnsureGraphicsBufferAllocated(ref m_DeferredLightingFeatureTilesListsBuffer, GraphicsBuffer.Target.Structured, 8 * num, 4, "DeferredLightingFeatureTilesLists");
	}
}
