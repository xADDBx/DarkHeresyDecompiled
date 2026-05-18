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

		public void ProcessTransformData(NativeArray<EntityId> transformedID, NativeArray<EntityId> parentID, NativeArray<Matrix4x4> localToWorldMatrices, NativeArray<Vector3> positions, NativeArray<Quaternion> rotations, NativeArray<Vector3> scales)
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

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
		{
			foreach (EntityId item in destroyedID)
			{
				int instanceID = item;
				m_BatchRendererGroup.RemoveRenderer(GPUDrivenInstanceID.UnityObject(instanceID));
			}
			if (changedID.Length <= 0)
			{
				return;
			}
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			NativeList<EntityId> nativeList = new NativeList<EntityId>(changedID.Length, Allocator.Temp);
			NativeList<int> nativeList2 = new NativeList<int>(changedID.Length, Allocator.Temp);
			NativeList<EntityId> nativeList3 = new NativeList<EntityId>(changedID.Length, Allocator.Temp);
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
						EntityId value = changedID[i];
						nativeList.Add(in value);
					}
					break;
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.Removed:
				{
					EntityId value = changedID[i];
					nativeList3.Add(in value);
					break;
				}
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.CouldNotRegister:
				{
					int value2 = changedID[i];
					nativeList2.Add(in value2);
					break;
				}
				case GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported:
				{
					EntityId value = changedID[i];
					nativeList3.Add(in value);
					int value2 = changedID[i];
					nativeList2.Add(in value2);
					break;
				}
				}
			}
			NativeList<EntityId> invalidRendererIDs = new NativeList<EntityId>(Allocator.Temp);
			m_BatchRendererGroup.EnableGPUDrivenRenderingAndLoadNativeData(nativeList.AsArray(), in changeContext, invalidRendererIDs);
			m_BatchRendererGroup.DisableGPURendering(nativeList3.AsArray());
			foreach (EntityId item2 in invalidRendererIDs)
			{
				int value3 = item2;
				m_BatchRendererGroup.RemoveRenderer(GPUDrivenInstanceID.UnityObject(value3), GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported);
				nativeList2.Add(in value3);
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

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
		{
			foreach (EntityId item in destroyedID)
			{
				int instanceID = item;
				if (m_BatchRendererGroup.TryMapMeshFilterInstanceIDToMeshRenderer(GPUDrivenInstanceID.UnityObject(instanceID), out var rendererInstanceID))
				{
					m_BatchRendererGroup.RemoveRenderer(rendererInstanceID);
				}
			}
			if (changedID.Length <= 0)
			{
				return;
			}
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			NativeList<EntityId> nativeList = new NativeList<EntityId>(changedID.Length, Allocator.Temp);
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
						EntityId value = changedID[i];
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

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
		{
			if (changedID.Length == 0 && destroyedID.Length == 0)
			{
				return;
			}
			GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
			NativeList<EntityId> nativeList = new NativeList<EntityId>(changedID.Length + destroyedID.Length, Allocator.Temp);
			foreach (EntityId item in destroyedID)
			{
				int instanceID = item;
				if (!m_BatchRendererGroup.TryMapGPUDrivenRendererInstanceIDToMeshRenderer(GPUDrivenInstanceID.UnityObject(instanceID), out var rendererInstanceID))
				{
					continue;
				}
				int rawInstanceID = rendererInstanceID.RawInstanceID;
				MeshRenderer meshRenderer = ObjectDispatcherService.FindByInstanceId<MeshRenderer>(rawInstanceID);
				if (meshRenderer != null)
				{
					GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = GPUDrivenBatchRendererGroup.RendererDesc.FromMeshRenderer(meshRenderer);
					GPUDrivenBatchRendererGroup batchRendererGroup = m_BatchRendererGroup;
					GPUDrivenBatchRendererGroup.RendererParams rendererParams = default(GPUDrivenBatchRendererGroup.RendererParams);
					GPUDrivenBatchRendererGroup.RendererUpdateStatus rendererUpdateStatus = batchRendererGroup.AddOrUpdateRenderer(in rendererDesc, in changeContext, GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData, in rendererParams);
					if (rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.Removed || rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.RemovedAsUnsupported)
					{
						EntityId value = rawInstanceID;
						nativeList.Add(in value);
					}
				}
				else
				{
					m_BatchRendererGroup.RemoveRenderer(GPUDrivenInstanceID.UnityObject(rawInstanceID));
					EntityId value = rawInstanceID;
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
						EntityId value = changedID[i];
						nativeList.Add(in value);
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

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
		{
			if (changed.Count <= 0)
			{
				return;
			}
			bool flag = true;
			foreach (EntityId item2 in changedID)
			{
				int item = item2;
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

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
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

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
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
