using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenBRGObjectTracker : IDisposable
{
	private sealed class MeshRendererTracker : ObjectTracker<MeshRenderer>, IObjectTransformTracker
	{
		private static class Profiling
		{
			public static ProfilingSampler ProcessTransformData = new ProfilingSampler("ProcessTransformData");
		}

		private readonly GPUDrivenBatchRendererGroup m_BatchRendererGroup;

		public MeshRendererTracker(GPUDrivenBatchRendererGroup batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags trackingFlags = ObjectDispatcherService.TypeTrackingFlags.Default)
			: base(trackingFlags)
		{
			m_BatchRendererGroup = batchRendererGroup;
		}

		public void ProcessTransformData(NativeArray<int> transformedID, NativeArray<int> parentID, NativeArray<Matrix4x4> localToWorldMatrices, NativeArray<Vector3> positions, NativeArray<Quaternion> rotations, NativeArray<Vector3> scales)
		{
			using (new ProfilingScope(Profiling.ProcessTransformData))
			{
				if (transformedID.Length != 0)
				{
					m_BatchRendererGroup.BatchUpdateRendererTransforms(transformedID, localToWorldMatrices, out var actuallyChangedIDs);
					GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
					m_BatchRendererGroup.EnableGPUDrivenRenderingAndLoadNativeData(actuallyChangedIDs.AsArray(), in changeContext);
					actuallyChangedIDs.Dispose();
				}
			}
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
		{
			foreach (int item in destroyedID)
			{
				m_BatchRendererGroup.RemoveRenderer(GPUDrivenInstanceID.UnityObject(item));
			}
			if (changedID.Length <= 0)
			{
				return;
			}
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			NativeList<int> nativeList = new NativeList<int>(changedID.Length, Allocator.Temp);
			NativeList<int> nativeList2 = new NativeList<int>(changedID.Length, Allocator.Temp);
			NativeList<int> nativeList3 = new NativeList<int>(changedID.Length, Allocator.Temp);
			for (int i = 0; i < changed.Count; i++)
			{
				MeshRenderer meshRenderer = (MeshRenderer)changed[i];
				GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = GPUDrivenBatchRendererGroup.RendererDesc.FromMeshRenderer(meshRenderer);
				GPUDrivenBatchRendererGroup batchRendererGroup = m_BatchRendererGroup;
				GPUDrivenBatchRendererGroup.RendererParams rendererParams = default(GPUDrivenBatchRendererGroup.RendererParams);
				switch (batchRendererGroup.AddOrUpdateRenderer(in rendererDesc, in changeContext, GPUDrivenRendererGroupPool.RendererUpdateFlags.RendererData | GPUDrivenRendererGroupPool.RendererUpdateFlags.MaterialData, in rendererParams))
				{
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.UpdatedRegistered:
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.Added:
					if (GPUDrivenRenderingUtils.IsRendererEnabled(meshRenderer))
					{
						int value = changedID[i];
						nativeList.Add(in value);
					}
					break;
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.Removed:
				{
					int value = changedID[i];
					nativeList3.Add(in value);
					break;
				}
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.CouldNotRegister:
				{
					int value = changedID[i];
					nativeList2.Add(in value);
					break;
				}
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported:
				{
					int value = changedID[i];
					nativeList3.Add(in value);
					value = changedID[i];
					nativeList2.Add(in value);
					break;
				}
				}
			}
			NativeList<int> invalidRendererIDs = new NativeList<int>(Allocator.Temp);
			m_BatchRendererGroup.EnableGPUDrivenRenderingAndLoadNativeData(nativeList.AsArray(), in changeContext, invalidRendererIDs);
			m_BatchRendererGroup.DisableGPURendering(nativeList3.AsArray());
			foreach (int item2 in invalidRendererIDs)
			{
				int value2 = item2;
				m_BatchRendererGroup.RemoveRenderer(GPUDrivenInstanceID.UnityObject(value2), GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported);
				nativeList2.Add(in value2);
			}
			if (nativeList2.Length > 0)
			{
				RequestLightmapsForUnsupportedRenderers(nativeList2.AsArray());
			}
		}
	}

	private sealed class MeshFilterTracker : ObjectTracker<MeshFilter>
	{
		private readonly GPUDrivenBatchRendererGroup m_BatchRendererGroup;

		public MeshFilterTracker(GPUDrivenBatchRendererGroup batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags trackingFlags = ObjectDispatcherService.TypeTrackingFlags.Default)
			: base(trackingFlags)
		{
			m_BatchRendererGroup = batchRendererGroup;
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
		{
			foreach (int item in destroyedID)
			{
				if (m_BatchRendererGroup.TryMapMeshFilterInstanceIDToMeshRenderer(GPUDrivenInstanceID.UnityObject(item), out var rendererInstanceID))
				{
					m_BatchRendererGroup.RemoveRenderer(rendererInstanceID);
				}
			}
			if (changedID.Length <= 0)
			{
				return;
			}
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			NativeList<int> nativeList = new NativeList<int>(changedID.Length, Allocator.Temp);
			for (int i = 0; i < changed.Count; i++)
			{
				MeshFilter meshFilter = (MeshFilter)changed[i];
				if (meshFilter.TryGetComponent<MeshRenderer>(out var component))
				{
					GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = GPUDrivenBatchRendererGroup.RendererDesc.FromMeshRenderer(component, meshFilter);
					GPUDrivenBatchRendererGroup batchRendererGroup = m_BatchRendererGroup;
					GPUDrivenBatchRendererGroup.RendererParams rendererParams = default(GPUDrivenBatchRendererGroup.RendererParams);
					GPUDrivenBatchRendererGroup.RendererUpdateStatus rendererUpdateStatus = batchRendererGroup.UpdateRendererMesh(in rendererDesc, in changeContext, in rendererParams);
					if (rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.Removed || rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported)
					{
						int value = changedID[i];
						nativeList.Add(in value);
					}
				}
			}
			m_BatchRendererGroup.DisableGPURendering(nativeList.AsArray());
		}
	}

	private sealed class GPUDrivenRendererTracker : ObjectTracker<GPUDrivenRenderer>
	{
		private readonly GPUDrivenBatchRendererGroup m_BatchRendererGroup;

		public GPUDrivenRendererTracker(GPUDrivenBatchRendererGroup batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags trackingFlags = ObjectDispatcherService.TypeTrackingFlags.Default)
			: base(trackingFlags)
		{
			m_BatchRendererGroup = batchRendererGroup;
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
		{
			if (changedID.Length == 0 && destroyedID.Length == 0)
			{
				return;
			}
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			NativeList<int> nativeList = new NativeList<int>(changedID.Length + destroyedID.Length, Allocator.Temp);
			foreach (int item in destroyedID)
			{
				if (!m_BatchRendererGroup.TryMapGPUDrivenRendererInstanceIDToMeshRenderer(GPUDrivenInstanceID.UnityObject(item), out var rendererInstanceID))
				{
					continue;
				}
				int value = rendererInstanceID.RawInstanceID;
				MeshRenderer meshRenderer = ObjectDispatcherService.FindByInstanceId<MeshRenderer>(value);
				if (meshRenderer != null)
				{
					GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = GPUDrivenBatchRendererGroup.RendererDesc.FromMeshRenderer(meshRenderer);
					GPUDrivenBatchRendererGroup batchRendererGroup = m_BatchRendererGroup;
					GPUDrivenBatchRendererGroup.RendererParams rendererParams = default(GPUDrivenBatchRendererGroup.RendererParams);
					GPUDrivenBatchRendererGroup.RendererUpdateStatus rendererUpdateStatus = batchRendererGroup.AddOrUpdateRenderer(in rendererDesc, in changeContext, GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData, in rendererParams);
					if (rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.Removed || rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported)
					{
						nativeList.Add(in value);
					}
				}
				else
				{
					m_BatchRendererGroup.RemoveRenderer(GPUDrivenInstanceID.UnityObject(value));
					nativeList.Add(in value);
				}
			}
			for (int i = 0; i < changed.Count; i++)
			{
				if (((GPUDrivenRenderer)changed[i]).TryGetComponent<MeshRenderer>(out var component))
				{
					GPUDrivenBatchRendererGroup.RendererDesc rendererDesc2 = GPUDrivenBatchRendererGroup.RendererDesc.FromMeshRenderer(component);
					GPUDrivenBatchRendererGroup batchRendererGroup2 = m_BatchRendererGroup;
					GPUDrivenBatchRendererGroup.RendererParams rendererParams = default(GPUDrivenBatchRendererGroup.RendererParams);
					GPUDrivenBatchRendererGroup.RendererUpdateStatus rendererUpdateStatus2 = batchRendererGroup2.AddOrUpdateRenderer(in rendererDesc2, in changeContext, GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData, in rendererParams);
					if (rendererUpdateStatus2 == GPUDrivenBatchRendererGroup.RendererUpdateStatus.Removed || rendererUpdateStatus2 == GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported)
					{
						int value2 = changedID[i];
						nativeList.Add(in value2);
					}
				}
			}
			m_BatchRendererGroup.DisableGPURendering(nativeList.AsArray());
		}
	}

	private sealed class MaterialTracker : ObjectTracker<Material>
	{
		private readonly GPUDrivenBatchRendererGroup m_BatchRendererGroup;

		private readonly HashSet<int> m_ProcessedMaterialIDs = new HashSet<int>();

		public MaterialTracker(GPUDrivenBatchRendererGroup batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags trackingFlags = ObjectDispatcherService.TypeTrackingFlags.Default)
			: base(trackingFlags)
		{
			m_BatchRendererGroup = batchRendererGroup;
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
		{
			if (changed.Count <= 0)
			{
				return;
			}
			bool flag = true;
			foreach (int item in changedID)
			{
				if (!m_ProcessedMaterialIDs.Add(item))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
				m_BatchRendererGroup.BatchProcessMaterialUpdate(changed, destroyedID, in changeContext);
			}
		}
	}

	private sealed class ShaderTracker : ObjectTracker<Shader>
	{
		private readonly GPUDrivenBatchRendererGroup m_BatchRendererGroup;

		private readonly HashSet<int> m_ProcessedShaderIDs = new HashSet<int>();

		public ShaderTracker(GPUDrivenBatchRendererGroup batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags trackingFlags = ObjectDispatcherService.TypeTrackingFlags.Default)
			: base(trackingFlags)
		{
			m_BatchRendererGroup = batchRendererGroup;
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
		{
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			for (int i = 0; i < changed.Count; i++)
			{
				Shader shader = (Shader)changed[i];
				if (!m_ProcessedShaderIDs.Add(changedID[i]))
				{
					m_BatchRendererGroup.ProcessShaderChanges(shader, in changeContext);
				}
			}
		}
	}

	private sealed class MeshTracker : ObjectTracker<Mesh>
	{
		private readonly GPUDrivenBatchRendererGroup m_BatchRendererGroup;

		public MeshTracker(GPUDrivenBatchRendererGroup batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags trackingFlags = ObjectDispatcherService.TypeTrackingFlags.Default)
			: base(trackingFlags)
		{
			m_BatchRendererGroup = batchRendererGroup;
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
		{
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			if (destroyedID.Length > 0)
			{
				m_BatchRendererGroup.BatchProcessMeshDestruction(in changeContext);
			}
			if (changed.Count > 0)
			{
				m_BatchRendererGroup.BatchUpdateMeshAssets(changed, in changeContext);
			}
		}
	}

	private readonly MaterialTracker m_MaterialTracker;

	private readonly MeshFilterTracker m_MeshFilterTracker;

	private readonly MeshRendererTracker m_MeshRendererTracker;

	private readonly MeshTracker m_MeshTracker;

	private readonly GPUDrivenRendererTracker m_RendererTracker;

	private readonly ShaderTracker m_ShaderTracker;

	public GPUDrivenBRGObjectTracker(GPUDrivenBatchRendererGroup batchRendererGroup)
	{
		m_MeshRendererTracker = new MeshRendererTracker(batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags.SceneObjects | ObjectDispatcherService.TypeTrackingFlags.EditorOnlyObjects);
		m_MeshFilterTracker = new MeshFilterTracker(batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags.SceneObjects | ObjectDispatcherService.TypeTrackingFlags.EditorOnlyObjects);
		m_RendererTracker = new GPUDrivenRendererTracker(batchRendererGroup, ObjectDispatcherService.TypeTrackingFlags.SceneObjects | ObjectDispatcherService.TypeTrackingFlags.EditorOnlyObjects);
		m_MaterialTracker = new MaterialTracker(batchRendererGroup);
		m_ShaderTracker = new ShaderTracker(batchRendererGroup);
		m_MeshTracker = new MeshTracker(batchRendererGroup);
		ObjectDispatcherService.RegisterObjectTracker(m_MeshRendererTracker);
		ObjectDispatcherService.RegisterObjectTracker(m_MeshFilterTracker);
		ObjectDispatcherService.RegisterObjectTracker(m_RendererTracker);
		ObjectDispatcherService.RegisterObjectTracker(m_ShaderTracker);
		ObjectDispatcherService.RegisterObjectTracker(m_MaterialTracker);
		ObjectDispatcherService.RegisterObjectTracker(m_MeshTracker);
	}

	public void Dispose()
	{
		ObjectDispatcherService.UnregisterObjectTracker(m_MeshRendererTracker);
		ObjectDispatcherService.UnregisterObjectTracker(m_MeshFilterTracker);
		ObjectDispatcherService.UnregisterObjectTracker(m_RendererTracker);
		ObjectDispatcherService.UnregisterObjectTracker(m_MaterialTracker);
		ObjectDispatcherService.UnregisterObjectTracker(m_ShaderTracker);
		ObjectDispatcherService.UnregisterObjectTracker(m_MeshTracker);
	}

	private static void RequestLightmapsForUnsupportedRenderers(NativeArray<int> unsupportedInstanceIDs)
	{
		List<UnityEngine.Object> value;
		using (ListPool<UnityEngine.Object>.Get(out value))
		{
			Resources.InstanceIDToObjectList(unsupportedInstanceIDs, value);
			foreach (UnityEngine.Object item in value)
			{
				if (item is MeshRenderer meshRenderer)
				{
					GPUDrivenLightmapReference.RequestLightmapUnpack(meshRenderer.gameObject.scene, meshRenderer.lightmapIndex);
				}
			}
		}
	}
}
