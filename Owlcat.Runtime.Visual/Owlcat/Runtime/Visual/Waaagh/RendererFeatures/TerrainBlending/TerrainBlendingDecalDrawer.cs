using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.VirtualTerrain;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

internal sealed class TerrainBlendingDecalDrawer : ICustomDecalDrawer
{
	private struct TerrainInfo : IComparable<TerrainInfo>
	{
		public Material TerrainMaterialTemplate;

		public TerrainBlendingMaterialInfo TerrainMaterialInfo;

		public UnityEngine.Terrain Terrain;

		public TerrainData TerrainData;

		public OwlcatVirtualTerrain OwlcatTerrain;

		public float Distance;

		public Matrix4x4 Matrix;

		public Vector4 Size;

		public int CompareTo(TerrainInfo other)
		{
			if (Distance > other.Distance)
			{
				return 1;
			}
			if (Distance < other.Distance)
			{
				return -1;
			}
			return 0;
		}
	}

	private const int kFrustumPlanesCount = 6;

	private const int kFrustumPointsCount = 8;

	private static readonly ProfilingSampler ProfilingSampler = new ProfilingSampler("Terrain Blending");

	private static readonly int _TerrainSize = Shader.PropertyToID("_TerrainSize");

	private static readonly int _TerrainHeightmapTexture = Shader.PropertyToID("_TerrainHeightmapTexture");

	private static readonly int _TerrainNormalmapTexture = Shader.PropertyToID("_TerrainNormalmapTexture");

	private static readonly int _TerrainBlendingOffset = Shader.PropertyToID("_TerrainBlendingOffset");

	private static readonly int _TerrainBlendingDepth = Shader.PropertyToID("_TerrainBlendingDepth");

	private static readonly int _TerrainBlendingNoiseTiling = Shader.PropertyToID("_TerrainBlendingNoiseTiling");

	private static readonly int _TerrainBlendingNoiseStrength = Shader.PropertyToID("_TerrainBlendingNoiseStrength");

	private static readonly int _TerrainBlendingHeightmapMad = Shader.PropertyToID("_TerrainBlendingHeightmapMad");

	private static readonly int _TerrainBlendingSurfaceNormalBlendParams = Shader.PropertyToID("_TerrainBlendingSurfaceNormalBlendParams");

	private static readonly int _TerrainBlendingDebugMode = Shader.PropertyToID("_TerrainBlendingDebugMode");

	private static readonly int _TerrainBlendingRenderingLayerMask = Shader.PropertyToID("_TerrainBlendingRenderingLayerMask");

	private static Plane[] s_TempPlanes;

	private static Vector3[] s_TempPoints;

	private readonly TerrainBlendingFeature m_Feature;

	private readonly MaterialPropertyBlock m_Properties;

	private readonly Mesh m_TerrainDecalMesh;

	private readonly List<TerrainInfo> m_TerrainInfos = new List<TerrainInfo>();

	[NonSerialized]
	private NativeArray<float4> m_CullingJobFrustumPlanes;

	[NonSerialized]
	private NativeArray<float3> m_CullingJobFrustumPoints;

	[NonSerialized]
	private NativeArray<TerrainCullingJob.Box> m_CullingJobTerrainBoundingBoxes;

	[NonSerialized]
	private NativeArray<bool> m_CullingJobTerrainVisibleStatuses;

	[NonSerialized]
	private JobHandle m_CullingJobHandle;

	[NonSerialized]
	private bool m_CullingJobScheduled;

	[NonSerialized]
	private TerrainBlendingMaterialInfoCache m_MaterialInfoCache = new TerrainBlendingMaterialInfoCache();

	public TerrainBlendingDecalDrawer(TerrainBlendingFeature feature)
	{
		m_Feature = feature;
		m_Properties = new MaterialPropertyBlock();
		m_TerrainDecalMesh = TerrainBlendingDecalMeshFactory.Create();
	}

	public void OnStartSetupJobs(Camera camera)
	{
		ScheduleCullingJob(camera);
	}

	public void OnCompleteSetupJobs()
	{
		CompleteCullingJob();
	}

	public bool HasAnyScheduledWorkload()
	{
		return m_CullingJobScheduled;
	}

	public bool CanBeCulled()
	{
		return !HasAnythingToDrawInternal();
	}

	public bool PreventParentPassCulling(ScriptableRenderContext context)
	{
		return HasAnythingToDrawInternal();
	}

	private bool HasAnythingToDrawInternal()
	{
		CompleteCullingJob();
		return m_TerrainInfos.Count > 0;
	}

	public void Draw(CommandBuffer cmd, ScriptableRenderContext context, CustomDecalSubset subset)
	{
		CompleteCullingJob();
		if (m_TerrainInfos.Count == 0 || subset != 0)
		{
			return;
		}
		try
		{
			using (new ProfilingScope(cmd, ProfilingSampler))
			{
				m_Properties.SetFloat(_TerrainBlendingOffset, m_Feature.BlendingOffset);
				m_Properties.SetFloat(_TerrainBlendingDepth, m_Feature.BlendingDepth);
				m_Properties.SetFloat(_TerrainBlendingNoiseTiling, m_Feature.BlendingNoiseTiling);
				m_Properties.SetFloat(_TerrainBlendingNoiseStrength, m_Feature.BlendingNoiseStrength);
				m_Properties.SetVector(_TerrainBlendingSurfaceNormalBlendParams, new Vector4(m_Feature.SurfaceNormalBlendSlopeRange.x, m_Feature.SurfaceNormalBlendSlopeRange.y, m_Feature.SurfaceNormalBlendFactor, 0f));
				if (m_Feature.DebugMode != 0)
				{
					cmd.EnableShaderKeyword("_TERRAIN_BLENDING_DEBUG");
					m_Properties.SetInteger(_TerrainBlendingDebugMode, (int)m_Feature.DebugMode);
				}
				else
				{
					cmd.DisableShaderKeyword("_TERRAIN_BLENDING_DEBUG");
				}
				TerrainInfo terrainInfo = m_TerrainInfos[0];
				cmd.SetGlobalInt(_TerrainBlendingRenderingLayerMask, (int)m_Feature.RenderingLayerMask.value);
				cmd.DrawProcedural(Matrix4x4.identity, terrainInfo.TerrainMaterialTemplate, terrainInfo.TerrainMaterialInfo.BlendingMaskPopulatePass, MeshTopology.Triangles, 3);
				foreach (TerrainInfo terrainInfo2 in m_TerrainInfos)
				{
					TerrainInfo info2 = terrainInfo2;
					SetupTerrainProperties(in info2);
					cmd.DrawMesh(m_TerrainDecalMesh, info2.Matrix, info2.TerrainMaterialTemplate, 0, info2.TerrainMaterialInfo.BlendingMaskReducePass, m_Properties);
				}
				foreach (TerrainInfo terrainInfo3 in m_TerrainInfos)
				{
					TerrainInfo info3 = terrainInfo3;
					SetupTerrainProperties(in info3);
					cmd.DrawMesh(m_TerrainDecalMesh, info3.Matrix, info3.TerrainMaterialTemplate, 0, info3.TerrainMaterialInfo.BlendingDecalPass, m_Properties);
				}
			}
		}
		finally
		{
			m_TerrainInfos.Clear();
			m_Properties.Clear();
		}
		void SetupTerrainProperties(in TerrainInfo info)
		{
			info.OwlcatTerrain.PopulateMaterialProperties(m_Properties);
			m_Properties.SetVector(_TerrainSize, info.Size);
			m_Properties.SetTexture(_TerrainHeightmapTexture, info.TerrainData.heightmapTexture);
			m_Properties.SetTexture(_TerrainNormalmapTexture, info.Terrain.normalmapTexture);
			m_Properties.SetVector(_TerrainBlendingHeightmapMad, new Vector4((float)(info.Terrain.terrainData.heightmapResolution - 1) / (float)info.Terrain.terrainData.heightmapResolution, 0.5f / (float)(info.Terrain.terrainData.heightmapResolution - 1), 0f, 0f));
		}
	}

	private void ScheduleCullingJob(Camera camera)
	{
		PopulateTerrainInfo(camera.transform.position);
		if (m_TerrainInfos.Count != 0)
		{
			m_CullingJobFrustumPlanes = GetFrustumPlanes(camera);
			m_CullingJobFrustumPoints = GetFrustumPoints(camera);
			m_CullingJobTerrainBoundingBoxes = GetTerrainBoundingBoxes(m_TerrainInfos);
			m_CullingJobTerrainVisibleStatuses = new NativeArray<bool>(m_TerrainInfos.Count, Allocator.TempJob);
			TerrainCullingJob terrainCullingJob = default(TerrainCullingJob);
			terrainCullingJob.FrustumPlanes = m_CullingJobFrustumPlanes;
			terrainCullingJob.FrustumPoints = m_CullingJobFrustumPoints;
			terrainCullingJob.TerrainBoundingBoxes = m_CullingJobTerrainBoundingBoxes;
			terrainCullingJob.TerrainVisibleStatuses = m_CullingJobTerrainVisibleStatuses;
			TerrainCullingJob jobData = terrainCullingJob;
			m_CullingJobHandle = jobData.Schedule();
			m_CullingJobScheduled = true;
		}
	}

	private void CompleteCullingJob()
	{
		if (!m_CullingJobScheduled)
		{
			return;
		}
		try
		{
			m_CullingJobHandle.Complete();
			for (int num = m_TerrainInfos.Count - 1; num >= 0; num--)
			{
				if (!m_CullingJobTerrainVisibleStatuses[num])
				{
					m_TerrainInfos.RemoveAt(num);
				}
			}
		}
		finally
		{
			m_CullingJobFrustumPlanes.Dispose();
			m_CullingJobFrustumPoints.Dispose();
			m_CullingJobTerrainBoundingBoxes.Dispose();
			m_CullingJobTerrainVisibleStatuses.Dispose();
			m_CullingJobHandle = default(JobHandle);
			m_CullingJobScheduled = false;
		}
	}

	private void PopulateTerrainInfo(Vector3 viewPosition)
	{
		m_TerrainInfos.Clear();
		Vector2 vector = new Vector2(viewPosition.x, viewPosition.z);
		List<UnityEngine.Terrain> value;
		using (ListPool<UnityEngine.Terrain>.Get(out value))
		{
			UnityEngine.Terrain.GetActiveTerrains(value);
			foreach (UnityEngine.Terrain item in value)
			{
				if (item.normalmapTexture == null)
				{
					continue;
				}
				TerrainData terrainData = item.terrainData;
				if (!(terrainData == null) && !(terrainData.heightmapTexture == null))
				{
					Material materialTemplate = item.materialTemplate;
					if (!(materialTemplate == null) && m_MaterialInfoCache.TryGetMaterialInfo(materialTemplate, out var result) && item.TryGetComponent<OwlcatVirtualTerrain>(out var component))
					{
						Vector3 position = item.GetPosition();
						Vector3 size = terrainData.size;
						Vector2 vector2 = new Vector2(position.x + size.x / 2f, position.z + size.z / 2f);
						float sqrMagnitude = (vector - vector2).sqrMagnitude;
						m_TerrainInfos.Add(new TerrainInfo
						{
							TerrainMaterialTemplate = materialTemplate,
							TerrainMaterialInfo = result,
							Terrain = item,
							TerrainData = terrainData,
							OwlcatTerrain = component,
							Distance = sqrMagnitude,
							Matrix = Matrix4x4.TRS(position, Quaternion.identity, size),
							Size = new Vector4(size.x, size.y, size.z, 0f)
						});
					}
				}
			}
		}
		m_TerrainInfos.Sort();
	}

	private static NativeArray<float4> GetFrustumPlanes(Camera camera)
	{
		NativeArray<float4> result = new NativeArray<float4>(6, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		try
		{
			if (s_TempPlanes == null)
			{
				s_TempPlanes = new Plane[6];
			}
			GeometryUtility.CalculateFrustumPlanes(camera, s_TempPlanes);
			for (int i = 0; i < 6; i++)
			{
				result[i] = new float4(s_TempPlanes[i].normal, s_TempPlanes[i].distance);
			}
			return result;
		}
		catch
		{
			result.Dispose();
			throw;
		}
	}

	private static NativeArray<float3> GetFrustumPoints(Camera camera)
	{
		NativeArray<float3> result = new NativeArray<float3>(8, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		try
		{
			if (s_TempPoints == null)
			{
				s_TempPoints = new Vector3[4];
			}
			Transform transform = camera.transform;
			camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, s_TempPoints);
			for (int i = 0; i < 4; i++)
			{
				result[i] = new float3(transform.TransformPoint(s_TempPoints[i]));
			}
			camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, s_TempPoints);
			for (int j = 0; j < 4; j++)
			{
				result[j + 4] = new float3(transform.TransformPoint(s_TempPoints[j]));
			}
			return result;
		}
		catch
		{
			result.Dispose();
			throw;
		}
	}

	private static NativeArray<TerrainCullingJob.Box> GetTerrainBoundingBoxes(List<TerrainInfo> terrainInfos)
	{
		NativeArray<TerrainCullingJob.Box> result = new NativeArray<TerrainCullingJob.Box>(terrainInfos.Count, Allocator.TempJob);
		try
		{
			TerrainCullingJob.Box value = default(TerrainCullingJob.Box);
			for (int i = 0; i < terrainInfos.Count; i++)
			{
				TerrainInfo terrainInfo = terrainInfos[i];
				value.Min = terrainInfo.Terrain.GetPosition();
				value.Max = value.Min + (float3)terrainInfo.TerrainData.size;
				result[i] = value;
			}
			return result;
		}
		catch
		{
			result.Dispose();
			throw;
		}
	}
}
