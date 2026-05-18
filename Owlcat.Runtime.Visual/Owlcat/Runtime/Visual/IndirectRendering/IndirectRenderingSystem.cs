using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.IndirectRendering.Details;
using Owlcat.Runtime.Visual.Terrain;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.IndirectRendering;

public class IndirectRenderingSystem
{
	public enum MaterialValidationError
	{
		MaterialIsNull,
		ShaderIsNull,
		KeywordIsDisabled,
		ShaderTagIsMissing
	}

	private struct ShaderReflectionInfo
	{
		public ShaderTagId[] PassLightModes;
	}

	private class GPUData
	{
		public IndirectArgs IndirectArgs;

		public NativeListWrapper<IndirectInstanceData> InstanceData;

		public NativeListWrapper<GPUDrivenInstanceID> InstanceIDs;

		public NativeListWrapper<GPUDrivenInstanceID> RegisteredInstanceIDs;

		public bool DynamicDataIsDirty;
	}

	public const string kKeyword = "INDIRECT_INSTANCING";

	public const string kShaderTag = "IndirectInstancing";

	private static readonly ShaderTagId s_LightModeTag;

	private static readonly Vector3[] s_AmbientProbeEvaluateDirection;

	private static readonly Color[] s_AmbientProbeEvaluateResult;

	private const string kIndirectDrawOpaqueTag = "Draw Indirect Pass";

	private const string kIndirectCullingPassTag = "Indirect Culling";

	private const int kIndirectArgsStride = 20;

	private const int kIndirectArgsStep = 5;

	private const int kInstanceBufferSizeDelta = 1024;

	private const int kMeshArgsStep = 4;

	private const GPUDrivenVisibilityFlags kGPUDrivenVisibilityFlags = GPUDrivenVisibilityFlags.ForceVisibleForCPUCulling;

	private readonly IndirectRenderingSystemSelectionHighlight m_SelectionHighlight = new IndirectRenderingSystemSelectionHighlight();

	private readonly IndirectRenderingSystemMaterialCache m_MaterialCache = new IndirectRenderingSystemMaterialCache();

	private readonly ProfilingSampler m_DrawProfilingSampler = new ProfilingSampler("Draw Indirect Pass");

	private readonly ProfilingSampler m_CullingProfilingSampler = new ProfilingSampler("Indirect Culling");

	private readonly ProfilingSampler m_PreRenderProfilingSampler = new ProfilingSampler("Indirect Culling");

	private ComputeBuffer m_ArgsBuffer;

	private ComputeBuffer m_MeshArgsBuffer;

	private ComputeBuffer m_InstanceDataBuffer;

	private ComputeBuffer m_IsVisibleBuffer;

	private ComputeBuffer m_MeshDataBuffer;

	private ComputeBuffer m_LightProbesBuffer;

	private readonly MaterialPropertyBlock m_MaterialPropertyBlock = new MaterialPropertyBlock();

	private bool m_IsDirty;

	private bool m_IsDirtyMeshFlags;

	private bool m_ShouldDirtyAfterPBD;

	private bool m_RequestCollectProbesOnSubmit;

	private IndirectRenderingSystemSettings m_Settings;

	private ComputeShader m_ShaderCulling;

	private int m_KernelClear;

	private int m_KernelCulling;

	private int m_KernelArgsCopy;

	private readonly Dictionary<IIndirectMesh, GPUData> m_MeshData = new Dictionary<IIndirectMesh, GPUData>();

	private readonly Dictionary<Material, Material> m_OverdrawMaterialCache = new Dictionary<Material, Material>();

	private readonly List<uint> m_ArgsList = new List<uint>();

	private readonly List<uint> m_MeshArgsList = new List<uint>();

	private readonly List<MeshData> m_DrawCallData = new List<MeshData>();

	private readonly List<Vector3> m_InstancePositions = new List<Vector3>();

	private readonly List<Color> m_InstanceLightProbeColor = new List<Color>();

	private readonly List<SphericalHarmonicsL2> m_LightProbes = new List<SphericalHarmonicsL2>();

	private readonly Vector3[] m_LightProbeEvaluationDir = new Vector3[1]
	{
		new Vector3(0f, 1f, 0f)
	};

	private readonly Color[] m_LightProbeEvaluationResult = new Color[1];

	private readonly Dictionary<Shader, ShaderReflectionInfo> m_ShaderReflectionInfos = new Dictionary<Shader, ShaderReflectionInfo>();

	private bool m_FirstInit = true;

	public Camera DebugCamera;

	private static readonly ShaderTagId[] s_TempShaderTagIds;

	public static IndirectRenderingSystem Instance { get; private set; }

	private bool UsingLightProbes
	{
		get
		{
			WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
			if (asset != null)
			{
				return asset.LightProbeSystem == LightProbeSystem.LegacyLightProbes;
			}
			return false;
		}
	}

	static IndirectRenderingSystem()
	{
		s_LightModeTag = new ShaderTagId("LightMode");
		s_AmbientProbeEvaluateDirection = new Vector3[1] { Vector3.up };
		s_AmbientProbeEvaluateResult = new Color[1] { Color.gray };
		s_TempShaderTagIds = new ShaderTagId[DrawingSettings.maxShaderPasses];
		Instance = new IndirectRenderingSystem();
	}

	private IndirectRenderingSystem()
	{
	}

	public void Initialize()
	{
		RenderRuntimeShaders renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeShaders>();
		m_Settings = GraphicsSettings.GetRenderPipelineSettings<IndirectRenderingSystemSettings>();
		m_ShaderCulling = renderPipelineSettings.IndirectRenderingCullShader;
		m_KernelClear = m_ShaderCulling.FindKernel("Clear");
		m_KernelCulling = m_ShaderCulling.FindKernel("Culling");
		m_KernelArgsCopy = m_ShaderCulling.FindKernel("ArgsCopy");
		LightProbes.tetrahedralizationCompleted += OnTetrahedralizationCompleted;
		m_FirstInit = false;
	}

	public void Cleanup()
	{
		m_MaterialCache.Clear();
		IIndirectMesh[] array = m_MeshData.Keys.ToArray();
		foreach (IIndirectMesh mesh in array)
		{
			UnregisterMesh(mesh);
		}
		if (m_ArgsBuffer != null)
		{
			m_ArgsBuffer.Release();
		}
		if (m_MeshArgsBuffer != null)
		{
			m_MeshArgsBuffer.Release();
		}
		if (m_InstanceDataBuffer != null)
		{
			m_InstanceDataBuffer.Release();
		}
		if (m_IsVisibleBuffer != null)
		{
			m_IsVisibleBuffer.Release();
		}
		if (m_MeshDataBuffer != null)
		{
			m_MeshDataBuffer.Release();
		}
		if (m_LightProbesBuffer != null)
		{
			m_LightProbesBuffer.Release();
		}
		m_MeshData.Clear();
		LightProbes.tetrahedralizationCompleted -= OnTetrahedralizationCompleted;
	}

	private void FindAndRegisterMeshes()
	{
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (monoBehaviour.enabled && monoBehaviour.gameObject.activeInHierarchy && monoBehaviour is IIndirectMesh indirectMesh)
			{
				RegisterMesh(indirectMesh);
				indirectMesh.UpdateInstances();
			}
		}
	}

	public void RegisterMesh(IIndirectMesh mesh)
	{
		if (mesh.Mesh == null || mesh.Materials.Count == 0 || m_MeshData.ContainsKey(mesh))
		{
			return;
		}
		int subMeshCount = mesh.Mesh.subMeshCount;
		GPUData gPUData = new GPUData();
		gPUData.IndirectArgs = new IndirectArgs(subMeshCount);
		for (int i = 0; i < subMeshCount; i++)
		{
			gPUData.IndirectArgs[i].BaseVertex = mesh.Mesh.GetBaseVertex(i);
			gPUData.IndirectArgs[i].IndexCountPerInstance = mesh.Mesh.GetIndexCount(i);
			gPUData.IndirectArgs[i].StartIndex = mesh.Mesh.GetIndexStart(i);
			gPUData.IndirectArgs[i].InstanceCount = 0u;
			gPUData.IndirectArgs[i].StartInstance = 0u;
		}
		gPUData.InstanceData = new NativeListWrapper<IndirectInstanceData>(Allocator.Persistent);
		gPUData.InstanceIDs = new NativeListWrapper<GPUDrivenInstanceID>(Allocator.Persistent);
		gPUData.RegisteredInstanceIDs = new NativeListWrapper<GPUDrivenInstanceID>(Allocator.Persistent);
		m_MeshData.Add(mesh, gPUData);
		if (mesh.IsDynamic)
		{
			for (int j = 0; j < mesh.MaxDynamicInstances; j++)
			{
				gPUData.InstanceData.Add(new IndirectInstanceData
				{
					hidden = 1u
				});
				gPUData.InstanceIDs.Add(GPUDrivenInstanceID.Invalid());
			}
		}
		m_IsDirty = true;
		m_RequestCollectProbesOnSubmit = true;
	}

	public void UnregisterMesh(IIndirectMesh mesh)
	{
		if (!m_MeshData.Remove(mesh, out var value))
		{
			return;
		}
		m_MaterialCache.ClearMeshMaterials(mesh);
		if (IsBRGEnabled(mesh))
		{
			foreach (GPUDrivenInstanceID inner in value.RegisteredInstanceIDs.GetInnerList())
			{
				if (inner.Type != 0)
				{
					GPUDrivenInstanceCommandQueue.ScheduleRemove(inner);
				}
			}
			value.RegisteredInstanceIDs.Dispose();
		}
		m_IsDirty = true;
	}

	private static bool IsBRGEnabled(IIndirectMesh mesh)
	{
		if (mesh.IsDynamic)
		{
			return false;
		}
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if (asset != null)
		{
			GPUDrivenBRGSettings gPUDrivenBRGSettings = asset.GPUDrivenBRGSettings;
			if (gPUDrivenBRGSettings != null && gPUDrivenBRGSettings.UseForIndirectRenderingSystem)
			{
				return gPUDrivenBRGSettings.IsEnabledAndSupported;
			}
			return false;
		}
		return false;
	}

	public bool RequiresInstanceIDs(IIndirectMesh mesh)
	{
		return IsBRGEnabled(mesh);
	}

	internal void SetMeshInstances(IIndirectMesh mesh, NativeListWrapper<IndirectInstanceData> instances, NativeListWrapper<GPUDrivenInstanceID> instanceIDs)
	{
		if (!m_MeshData.TryGetValue(mesh, out var value))
		{
			return;
		}
		bool flag = IsBRGEnabled(mesh);
		if (flag)
		{
			foreach (GPUDrivenInstanceID inner in value.RegisteredInstanceIDs.GetInnerList())
			{
				GPUDrivenInstanceCommandQueue.ScheduleRemove(inner);
			}
			value.RegisteredInstanceIDs.Clear();
		}
		if (value.InstanceData != instances)
		{
			value.InstanceData.Clear();
			value.InstanceData.AddRange(instances);
		}
		if (flag)
		{
			if (value.InstanceIDs != instanceIDs)
			{
				value.InstanceIDs.Clear();
				value.InstanceIDs.AddRange(instanceIDs);
			}
			List<Material> value2;
			using (ListPool<Material>.Get(out value2))
			{
				foreach (Material material in mesh.Materials)
				{
					value2.Add(material);
				}
				m_MaterialCache.SubstituteWithBRGMaterials(mesh, value2);
				for (int i = 0; i < value.InstanceData.Length; i++)
				{
					IndirectInstanceData indirectInstanceData = value.InstanceData[i];
					GPUDrivenInstanceID gPUDrivenInstanceID = value.InstanceIDs[i];
					Scene scene = mesh.Scene;
					ulong sceneCullingMask = mesh.SceneCullingMask;
					int instanceID = mesh.GameObject.GetInstanceID();
					if (gPUDrivenInstanceID.Type != 0 && indirectInstanceData.hidden == 0)
					{
						value.RegisteredInstanceIDs.Add(gPUDrivenInstanceID);
						GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = CreateGPUDrivenRendererDesc(gPUDrivenInstanceID);
						GPUDrivenBatchRendererGroup.RendererParams rendererParams = new GPUDrivenBatchRendererGroup.RendererParams
						{
							LocalToWorldMatrix = indirectInstanceData.objectToWorld,
							Materials = value2,
							Enabled = (indirectInstanceData.hidden == 0),
							Mesh = mesh.Mesh,
							Scene = scene,
							TransformScale = indirectInstanceData.objectToWorld.lossyScale,
							SceneCullingMask = sceneCullingMask,
							General = new GPUDrivenBatchRendererGroup.GeneralRendererSettings
							{
								Layer = 0,
								LightmapIndex = -1,
								OccluderBounds = default(Bounds),
								LightProbeUsage = LightProbeUsage.Off,
								RenderingLayerMask = (uint)mesh.RenderingLayerMask,
								ReceiveShadows = true,
								SortingOrder = 0,
								LightmapScaleOffset = default(float4),
								ShadowCastingMode = ShadowCastingMode.Off,
								StaticShadowCaster = false,
								MotionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion
							},
							GameObjectInstanceID = instanceID,
							PerInstanceProperties = CollectPerInstanceProperties(Allocator.Temp, in indirectInstanceData)
						};
						GPUDrivenInstanceCommandQueue.ScheduleAddOrUpdate(in rendererDesc, in rendererParams, GPUDrivenRendererGroupPool.RendererUpdateFlags.ForcedUpdate);
					}
				}
			}
		}
		if (mesh.IsDynamic)
		{
			value.DynamicDataIsDirty = true;
		}
		else
		{
			m_IsDirty = true;
		}
	}

	private static NativeArray<GPUDrivenRenderer.PropertyData> CollectPerInstanceProperties(Allocator allocator, in IndirectInstanceData indirectInstanceData, Vector4 bakedGI = default(Vector4))
	{
		NativeArray<GPUDrivenRenderer.PropertyData> result = new NativeArray<GPUDrivenRenderer.PropertyData>(3, allocator);
		result[0] = GPUDrivenRenderer.PropertyData.Vector(ShaderPropertyId._IndirectInstanceData_BakedGI, bakedGI);
		result[1] = GPUDrivenRenderer.PropertyData.Vector(ShaderPropertyId._IndirectInstanceData_Shadowmask, indirectInstanceData.shadowmask);
		result[2] = GPUDrivenRenderer.PropertyData.Vector(ShaderPropertyId._IndirectInstanceData_TintColor, indirectInstanceData.tintColor);
		return result;
	}

	internal NativeListWrapper<IndirectInstanceData> GetMeshInstances(IIndirectMesh mesh)
	{
		if (m_MeshData.TryGetValue(mesh, out var value))
		{
			return value.InstanceData;
		}
		return null;
	}

	internal NativeListWrapper<GPUDrivenInstanceID> GetInstanceIDs(IIndirectMesh mesh)
	{
		if (IsBRGEnabled(mesh) && m_MeshData.TryGetValue(mesh, out var value))
		{
			return value.InstanceIDs;
		}
		return null;
	}

	public void SetMeshDirty(IIndirectMesh mesh)
	{
		if (mesh.IsDynamic)
		{
			if (m_MeshData.TryGetValue(mesh, out var value))
			{
				value.DynamicDataIsDirty = true;
			}
		}
		else
		{
			m_IsDirty = true;
		}
	}

	public void Submit()
	{
		if (m_IsDirty)
		{
			m_IsDirty = false;
			m_IsDirtyMeshFlags = false;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
			{
				if (!IsBRGEnabled(meshDatum.Key))
				{
					_ = meshDatum.Key.IsDynamic;
					num += meshDatum.Value.InstanceData.Length;
					num2 += meshDatum.Key.Mesh.subMeshCount;
					num3++;
				}
			}
			if (m_ArgsBuffer == null || !m_ArgsBuffer.IsValid() || m_ArgsBuffer.count < num2 * 5)
			{
				if (m_ArgsBuffer != null)
				{
					m_ArgsBuffer.Release();
				}
				if (num2 > 0)
				{
					m_ArgsBuffer = new ComputeBuffer(5 * num2, 4, ComputeBufferType.DrawIndirect);
					m_ArgsBuffer.name = "Indirect Args";
				}
			}
			if (m_MeshArgsBuffer == null || !m_MeshArgsBuffer.IsValid() || m_MeshArgsBuffer.count < num3 * 4)
			{
				if (m_MeshArgsBuffer != null)
				{
					m_MeshArgsBuffer.Release();
				}
				if (m_MeshDataBuffer != null)
				{
					m_MeshDataBuffer.Release();
				}
				if (num3 > 0)
				{
					m_MeshArgsBuffer = new ComputeBuffer(4 * num3, 4, ComputeBufferType.Structured);
					m_MeshArgsBuffer.name = "Indirect Mesh Args Buffer";
					m_MeshDataBuffer = new ComputeBuffer(num3, Marshal.SizeOf(typeof(MeshData)), ComputeBufferType.Structured);
					m_MeshDataBuffer.name = "Indirect DrawCall Data";
				}
			}
			if (m_InstanceDataBuffer == null || !m_InstanceDataBuffer.IsValid() || m_InstanceDataBuffer.count < num)
			{
				if (m_InstanceDataBuffer != null && m_InstanceDataBuffer.IsValid())
				{
					m_InstanceDataBuffer.Release();
				}
				if (m_IsVisibleBuffer != null && m_IsVisibleBuffer.IsValid())
				{
					m_IsVisibleBuffer.Release();
				}
				if (m_LightProbesBuffer != null && m_LightProbesBuffer.IsValid())
				{
					m_LightProbesBuffer.Release();
				}
				if (num > 0)
				{
					int count = Mathf.CeilToInt((float)num / 1024f) * 1024;
					m_InstanceDataBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(IndirectInstanceData)), ComputeBufferType.Structured);
					m_InstanceDataBuffer.name = "Indirect Instance Data";
					m_IsVisibleBuffer = new ComputeBuffer(count, 4, ComputeBufferType.Structured);
					m_IsVisibleBuffer.name = "Indirect Visibility Buffer";
					m_LightProbesBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Color)), ComputeBufferType.Structured);
					m_LightProbesBuffer.name = "Indirect Light Probe Color Buffer";
					ResetLightProbesBuffer(m_LightProbesBuffer);
				}
			}
			if (m_RequestCollectProbesOnSubmit)
			{
				CollectLightProbes();
			}
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._IndirectInstanceDataBuffer, m_InstanceDataBuffer);
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._LightProbesBuffer, m_LightProbesBuffer);
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._IsVisibleBuffer, m_IsVisibleBuffer);
			if (num2 == 0 || num == 0)
			{
				return;
			}
			int num4 = 0;
			uint num5 = 0u;
			uint num6 = 0u;
			m_ArgsList.Clear();
			m_MeshArgsList.Clear();
			m_DrawCallData.Clear();
			foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum2 in m_MeshData)
			{
				if (IsBRGEnabled(meshDatum2.Key))
				{
					continue;
				}
				NativeListWrapper<IndirectInstanceData> instanceData = meshDatum2.Value.InstanceData;
				if (instanceData.Length > 0)
				{
					for (int i = 0; i < instanceData.Length; i++)
					{
						IndirectInstanceData indirectInstanceData = instanceData[i];
						indirectInstanceData.meshID = num5;
						instanceData[i] = indirectInstanceData;
					}
					m_InstanceDataBuffer.SetData(instanceData.AsArray(), 0, num4, instanceData.Length);
				}
				m_MeshArgsList.Add(num6);
				m_MeshArgsList.Add((uint)meshDatum2.Key.Mesh.subMeshCount);
				m_MeshArgsList.Add(0u);
				m_MeshArgsList.Add((uint)num4);
				IndirectArgs indirectArgs = meshDatum2.Value.IndirectArgs;
				for (int j = 0; j < indirectArgs.Entries.Length; j++)
				{
					indirectArgs.Entries[j].StartInstance = (uint)num4;
				}
				Bounds bounds = meshDatum2.Key.Mesh.bounds;
				MeshData meshData = default(MeshData);
				meshData.aabbMax = bounds.max;
				meshData.aabbMin = bounds.min;
				meshData.flags = meshDatum2.Key.Flags;
				meshData.unused = 0f;
				MeshData item = meshData;
				m_DrawCallData.Add(item);
				num4 += instanceData.Length;
				m_ArgsList.AddRange(indirectArgs.Args);
				num5++;
				num6 += (uint)meshDatum2.Key.Mesh.subMeshCount;
			}
			m_ArgsBuffer.SetData(m_ArgsList);
			m_MeshDataBuffer.SetData(m_DrawCallData);
			m_MeshArgsBuffer.SetData(m_MeshArgsList);
			return;
		}
		if (m_InstanceDataBuffer != null)
		{
			int num7 = 0;
			uint num8 = 0u;
			foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum3 in m_MeshData)
			{
				if (IsBRGEnabled(meshDatum3.Key))
				{
					continue;
				}
				NativeListWrapper<IndirectInstanceData> instanceData2 = meshDatum3.Value.InstanceData;
				if (instanceData2.Length > 0 && meshDatum3.Key.IsDynamic && meshDatum3.Value.DynamicDataIsDirty)
				{
					for (int k = 0; k < instanceData2.Length; k++)
					{
						IndirectInstanceData indirectInstanceData2 = instanceData2[k];
						indirectInstanceData2.meshID = num8;
						instanceData2[k] = indirectInstanceData2;
					}
					m_InstanceDataBuffer.SetData(instanceData2.AsArray(), 0, num7, instanceData2.Length);
					meshDatum3.Value.DynamicDataIsDirty = false;
				}
				num7 += instanceData2.Length;
				num8++;
			}
		}
		if (m_IsDirtyMeshFlags)
		{
			UpdateMeshDataFlags();
		}
	}

	public void SetMeshFlagsDirty()
	{
		m_IsDirtyMeshFlags = true;
	}

	private void UpdateMeshDataFlags()
	{
		int num = 0;
		foreach (var (indirectMesh2, _) in m_MeshData)
		{
			if (!IsBRGEnabled(indirectMesh2))
			{
				MeshData value = m_DrawCallData[num];
				value.flags = indirectMesh2.Flags;
				m_DrawCallData[num] = value;
				num++;
			}
		}
		if (num > 0)
		{
			m_MeshDataBuffer.SetData(m_DrawCallData);
		}
	}

	public void Cull(CommandBuffer cmd, Camera camera)
	{
		if (m_InstanceDataBuffer == null || m_ArgsBuffer == null || m_IsVisibleBuffer == null || !m_InstanceDataBuffer.IsValid() || !m_ArgsBuffer.IsValid() || !m_IsVisibleBuffer.IsValid())
		{
			return;
		}
		Camera camera2 = ((DebugCamera != null) ? DebugCamera : camera);
		Matrix4x4 worldToCameraMatrix = camera2.worldToCameraMatrix;
		Matrix4x4 val = (m_Settings.DistanceCullingEnabled ? Matrix4x4.Perspective(camera2.fieldOfView, camera2.aspect, camera2.nearClipPlane, m_Settings.CullingDistance) : camera2.projectionMatrix) * worldToCameraMatrix;
		int num = 0;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			num += meshDatum.Value.InstanceData.Length;
		}
		if (num == 0)
		{
			return;
		}
		using (new ProfilingScope(cmd, m_CullingProfilingSampler))
		{
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._MeshCount, m_MeshData.Count);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelClear, ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._ArgsBufferSize, m_ArgsBuffer.count);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelClear, ShaderPropertyId._MeshArgsBuffer, m_MeshArgsBuffer);
			cmd.DispatchCompute(m_ShaderCulling, m_KernelClear, RenderingUtils.DivRoundUp(m_MeshData.Count, 64), 1, 1);
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._TotalInstanceCount, num);
			cmd.SetComputeMatrixParam(m_ShaderCulling, ShaderPropertyId._CamViewProj, val);
			cmd.SetComputeVectorParam(m_ShaderCulling, ShaderPropertyId._CamPosition, camera2.transform.position);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._InstanceDataBuffer, m_InstanceDataBuffer);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._IsVisibleBuffer, m_IsVisibleBuffer);
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._IsVisibleBufferSize, m_IsVisibleBuffer.count);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._MeshDataBuffer, m_MeshDataBuffer);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._MeshArgsBuffer, m_MeshArgsBuffer);
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._MeshArgsBufferSize, m_MeshArgsBuffer.count);
			if (m_Settings.ShapeCullingEnabled && OwlcatTerrainTransition.Active)
			{
				float num2 = Mathf.Max(0f, OwlcatTerrainTransition.ShapeRadius - OwlcatTerrainTransition.ShapeBlendWidth / 2f);
				cmd.SetComputeVectorParam(m_ShaderCulling, ShaderPropertyId._IndirectInstancingShapeCullingParams, new Vector4(1f, num2 * num2, OwlcatTerrainTransition.ShapeCenter.x, OwlcatTerrainTransition.ShapeCenter.z));
			}
			else
			{
				cmd.SetComputeVectorParam(m_ShaderCulling, ShaderPropertyId._IndirectInstancingShapeCullingParams, default(Vector4));
			}
			cmd.DispatchCompute(m_ShaderCulling, m_KernelCulling, RenderingUtils.DivRoundUp(num, 64), 1, 1);
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._MeshCount, m_MeshData.Count);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelArgsCopy, ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
			cmd.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._ArgsBufferSize, m_ArgsBuffer.count);
			cmd.SetComputeBufferParam(m_ShaderCulling, m_KernelArgsCopy, ShaderPropertyId._MeshArgsBuffer, m_MeshArgsBuffer);
			cmd.DispatchCompute(m_ShaderCulling, m_KernelArgsCopy, RenderingUtils.DivRoundUp(m_MeshData.Count, 64), 1, 1);
		}
	}

	private void OnTetrahedralizationCompleted()
	{
		if (m_IsDirty)
		{
			m_RequestCollectProbesOnSubmit = true;
		}
		else
		{
			CollectLightProbes();
		}
	}

	public void CollectLightProbes()
	{
		m_RequestCollectProbesOnSubmit = false;
		if (!UsingLightProbes)
		{
			ResetLightProbesBuffer(m_LightProbesBuffer);
			return;
		}
		if (m_IsDirty)
		{
			Debug.LogWarning("IndirectRenderingSystem.CollectLightProbes was invoked while m_IsDirty == true");
		}
		int num = 0;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			if (IsBRGEnabled(meshDatum.Key))
			{
				if (meshDatum.Value.InstanceData.Length <= 0)
				{
					continue;
				}
				m_InstancePositions.Clear();
				foreach (IndirectInstanceData inner in meshDatum.Value.InstanceData.GetInnerList())
				{
					IndirectInstanceData instanceData = inner;
					m_InstancePositions.Add(GetWorldPosition(in instanceData));
				}
				LightProbes.CalculateInterpolatedLightAndOcclusionProbes(m_InstancePositions, m_LightProbes, null);
				for (int i = 0; i < m_LightProbes.Count; i++)
				{
					m_LightProbes[i].Evaluate(m_LightProbeEvaluationDir, m_LightProbeEvaluationResult);
					IndirectInstanceData indirectInstanceData = meshDatum.Value.InstanceData[i];
					GPUDrivenInstanceID instanceID = meshDatum.Value.InstanceIDs[i];
					if (instanceID.Type != 0 && indirectInstanceData.hidden == 0)
					{
						GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = CreateGPUDrivenRendererDesc(instanceID);
						GPUDrivenBatchRendererGroup.RendererParams rendererParams = new GPUDrivenBatchRendererGroup.RendererParams
						{
							PerInstanceProperties = CollectPerInstanceProperties(Allocator.Temp, in indirectInstanceData, m_LightProbeEvaluationResult[0])
						};
						GPUDrivenInstanceCommandQueue.ScheduleAddOrUpdate(in rendererDesc, in rendererParams, GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData);
					}
				}
			}
			else
			{
				if (m_LightProbesBuffer == null || !m_LightProbesBuffer.IsValid() || meshDatum.Value.InstanceData.Length <= 0)
				{
					continue;
				}
				m_InstancePositions.Clear();
				m_LightProbes.Clear();
				m_InstanceLightProbeColor.Clear();
				foreach (IndirectInstanceData inner2 in meshDatum.Value.InstanceData.GetInnerList())
				{
					IndirectInstanceData instanceData2 = inner2;
					m_InstancePositions.Add(GetWorldPosition(in instanceData2));
				}
				LightProbes.CalculateInterpolatedLightAndOcclusionProbes(m_InstancePositions, m_LightProbes, null);
				foreach (SphericalHarmonicsL2 lightProbe in m_LightProbes)
				{
					lightProbe.Evaluate(m_LightProbeEvaluationDir, m_LightProbeEvaluationResult);
					m_InstanceLightProbeColor.Add(m_LightProbeEvaluationResult[0]);
				}
				if (m_LightProbesBuffer.count < m_InstanceLightProbeColor.Count + num || m_InstanceLightProbeColor.Count == 0)
				{
					break;
				}
				m_LightProbesBuffer.SetData(m_InstanceLightProbeColor, 0, num, m_InstanceLightProbeColor.Count);
				num += m_InstanceLightProbeColor.Count;
			}
		}
	}

	private static GPUDrivenBatchRendererGroup.RendererDesc CreateGPUDrivenRendererDesc(GPUDrivenInstanceID instanceID)
	{
		return GPUDrivenBatchRendererGroup.RendererDesc.Custom(instanceID, GPUDrivenVisibilityFlags.ForceVisibleForCPUCulling);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3 GetWorldPosition(in IndirectInstanceData instanceData)
	{
		Matrix4x4 objectToWorld = instanceData.objectToWorld;
		return objectToWorld.GetColumn(3);
	}

	public void DrawPass(CommandBuffer cmd, CameraType cameraType, bool isIndirectRenderingEnabled, bool isSceneViewInPrefabEditMode, RendererListParams rendererListParams, bool debugOverdraw)
	{
		ShaderTagId[] array = s_TempShaderTagIds;
		int num = 0;
		for (int i = 0; i < DrawingSettings.maxShaderPasses; i++)
		{
			ShaderTagId shaderPassName = rendererListParams.drawSettings.GetShaderPassName(i);
			if (shaderPassName == ShaderTagId.none)
			{
				break;
			}
			array[num] = shaderPassName;
			num++;
		}
		if (num != 0)
		{
			DrawPass(cmd, cameraType, isIndirectRenderingEnabled, isSceneViewInPrefabEditMode, debugOverdraw, rendererListParams.filteringSettings.renderQueueRange, array.AsSpan(0, num));
		}
	}

	public void DrawPass(CommandBuffer cmd, CameraType cameraType, bool isIndirectRenderingEnabled, bool isSceneViewInPrefabEditMode, bool debugOverdraw, RenderQueueRange renderQueueRange, Span<ShaderTagId> lightModes, Material overrideDebugMaterial = null, int overrideDebugMaterialPass = 0)
	{
		if (!isIndirectRenderingEnabled || (cameraType != CameraType.Game && cameraType != CameraType.SceneView) || (cameraType == CameraType.SceneView && isSceneViewInPrefabEditMode))
		{
			return;
		}
		m_SelectionHighlight.Reset();
		if (m_ArgsBuffer == null || !m_ArgsBuffer.IsValid() || m_InstanceDataBuffer == null || !m_InstanceDataBuffer.IsValid())
		{
			return;
		}
		if (m_MeshData.Count > 0)
		{
			RenderingUtils.SetDefaultReflectionProbe(m_MaterialPropertyBlock);
		}
		int num = 0;
		int num2 = 4;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			if (IsBRGEnabled(meshDatum.Key))
			{
				continue;
			}
			if (meshDatum.Value.InstanceData.Length == 0 || meshDatum.Key.Materials.Count == 0)
			{
				num += 20 * meshDatum.Key.Mesh.subMeshCount;
				num2 += 5 * meshDatum.Key.Mesh.subMeshCount;
				continue;
			}
			IIndirectMesh key = meshDatum.Key;
			int subMeshCount = key.Mesh.subMeshCount;
			m_SelectionHighlight.RequestColor(key);
			for (int i = 0; i < subMeshCount; i++)
			{
				int num3 = i;
				if (num3 > key.Materials.Count - 1)
				{
					num3 = key.Materials.Count - 1;
				}
				Material material = key.Materials[num3];
				if (material != null && material.renderQueue >= renderQueueRange.lowerBound && material.renderQueue <= renderQueueRange.upperBound && TryGetShaderReflectionInfo(material.shader, out var result))
				{
					Span<ShaderTagId> span = lightModes;
					for (int j = 0; j < span.Length; j++)
					{
						ShaderTagId value = span[j];
						int num4 = Array.IndexOf(result.PassLightModes, value);
						if (num4 >= 0)
						{
							m_MaterialPropertyBlock.SetInt(ShaderPropertyId._ArgsOffset, num2);
							float4 @float = math.asfloat(new uint4((uint)meshDatum.Key.RenderingLayerMask));
							m_MaterialPropertyBlock.SetVector(ShaderPropertyId.unity_RenderingLayer, @float);
							if ((num < m_ArgsBuffer.count * 20) ? true : false)
							{
								cmd.DrawMeshInstancedIndirect(meshDatum.Key.Mesh, i, material, num4, m_ArgsBuffer, num, m_MaterialPropertyBlock);
							}
						}
					}
				}
				num += 20;
				num2 += 5;
			}
		}
	}

	public bool HasOpaqueObjects()
	{
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			foreach (Material material in meshDatum.Key.Materials)
			{
				if (material.HasProperty(ShaderPropertyId._Surface) && material.HasProperty(ShaderPropertyId._DistortionEnabled) && material.GetFloat(ShaderPropertyId._Surface) < 1f && material.GetFloat(ShaderPropertyId._DistortionEnabled) < 1f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasOpaqueDistortion()
	{
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			foreach (Material material in meshDatum.Key.Materials)
			{
				if (material.HasProperty(ShaderPropertyId._Surface) && material.HasProperty(ShaderPropertyId._DistortionEnabled) && material.GetFloat(ShaderPropertyId._Surface) < 1f && material.GetFloat(ShaderPropertyId._DistortionEnabled) > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTransparentObjects()
	{
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			foreach (Material material in meshDatum.Key.Materials)
			{
				if (material.HasProperty(ShaderPropertyId._Surface) && material.GetFloat(ShaderPropertyId._Surface) > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public IndirectRenderingStatisticsInfo GetStats()
	{
		int num = 0;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			num += meshDatum.Value.InstanceData.Length;
		}
		if (num == 0)
		{
			return default(IndirectRenderingStatisticsInfo);
		}
		IndirectRenderingStatisticsInfo result = default(IndirectRenderingStatisticsInfo);
		result.ArgsBufferCount = m_ArgsBuffer.count;
		result.MeshBufferCount = m_MeshArgsBuffer.count;
		result.InstanceBufferCount = m_InstanceDataBuffer.count;
		result.DrawcallCount = m_ArgsBuffer.count / 5;
		result.MeshCount = m_MeshArgsBuffer.count / 4;
		result.InstanceCount = num;
		return result;
	}

	public void PreRender()
	{
	}

	private bool TryGetShaderReflectionInfo(Shader shader, out ShaderReflectionInfo result)
	{
		if (shader != null)
		{
			if (!m_ShaderReflectionInfos.TryGetValue(shader, out var value))
			{
				int passCount = shader.passCount;
				ShaderReflectionInfo shaderReflectionInfo = default(ShaderReflectionInfo);
				shaderReflectionInfo.PassLightModes = new ShaderTagId[passCount];
				value = shaderReflectionInfo;
				for (int i = 0; i < passCount; i++)
				{
					value.PassLightModes[i] = shader.FindPassTagValue(i, s_LightModeTag);
				}
				m_ShaderReflectionInfos[shader] = value;
			}
			result = value;
			return true;
		}
		result = default(ShaderReflectionInfo);
		return false;
	}

	public static bool ValidateMaterialCompatibility(Material material, out MaterialValidationError error)
	{
		if (material == null)
		{
			error = MaterialValidationError.MaterialIsNull;
			return false;
		}
		Shader shader = material.shader;
		if (shader == null)
		{
			error = MaterialValidationError.ShaderIsNull;
			return false;
		}
		LocalKeyword keyword = shader.keywordSpace.FindKeyword("INDIRECT_INSTANCING");
		if (keyword.isValid)
		{
			if (!material.IsKeywordEnabled(in keyword))
			{
				error = MaterialValidationError.KeywordIsDisabled;
				return false;
			}
		}
		else if (material.GetTag("IndirectInstancing", searchFallbacks: false) != "true")
		{
			error = MaterialValidationError.ShaderTagIsMissing;
			return false;
		}
		error = (MaterialValidationError)(-1);
		return true;
	}

	private static void ResetLightProbesBuffer(ComputeBuffer lightProbesBuffer)
	{
		if (lightProbesBuffer != null && lightProbesBuffer.IsValid())
		{
			NativeArray<Color> array = new NativeArray<Color>(lightProbesBuffer.count, Allocator.Temp);
			RenderSettings.ambientProbe.Evaluate(s_AmbientProbeEvaluateDirection, s_AmbientProbeEvaluateResult);
			array.FillArray(in s_AmbientProbeEvaluateResult[0]);
			lightProbesBuffer.SetData(array);
			array.Dispose();
		}
	}
}
