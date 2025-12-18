using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenBatchRendererGroup : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private static class Profiling
	{
		public static readonly ProfilingSampler BatchUpdateRendererTransforms = new ProfilingSampler("GPUDrivenBRG BatchUpdateRendererTransforms");

		public static readonly ProfilingSampler UpdateTransformJobsCollectNativeData = new ProfilingSampler("GPUDrivenBRG UpdateTransformJobs CollectNativeData");

		private const string kPrefix = "GPUDrivenBRG ";

		public static readonly ProfilingSampler OnPerformCulling = new ProfilingSampler("GPUDrivenBRG OnPerformCulling");

		public static readonly ProfilingSampler LastFrameTransformUpdates = new ProfilingSampler("GPUDrivenBRG LastFrameTransformUpdates");

		public static readonly ProfilingSampler PreRender = new ProfilingSampler("GPUDrivenBRG PreRender");

		public static readonly ProfilingSampler PostRender = new ProfilingSampler("GPUDrivenBRG PostRender");

		public static readonly ProfilingSampler AddOrUpdateRenderer = new ProfilingSampler("GPUDrivenBRG AddOrUpdateRenderer");

		public static readonly ProfilingSampler UpdateNativeDataValidation = new ProfilingSampler("GPUDrivenBRG UpdateNativeDataValidation");

		public static readonly ProfilingSampler UpdateNativeData = new ProfilingSampler("GPUDrivenBRG UpdateNativeData");
	}

	private static class UpdateTransformJobs
	{
		public struct TransformNativeData
		{
			public GPUDrivenSceneHandle SceneHandle;
		}

		public struct TransformRendererState
		{
			public int OriginalIndex;

			public GPUDrivenInstanceID InstanceID;

			public RendererState RendererState;
		}

		[BurstCompile]
		public struct CollectRendererStatesJob : IJobParallelForBatch
		{
			public const int kBatchSize = 32;

			[ReadOnly]
			public NativeHashMap<GPUDrivenInstanceID, RendererState> InstanceIDToRendererState;

			[ReadOnly]
			[NativeDisableContainerSafetyRestriction]
			public NativeArray<int> InstanceIDs;

			[ReadOnly]
			[NativeDisableContainerSafetyRestriction]
			public NativeList<TransformRendererState>.ParallelWriter Result;

			public NativeList<int>.ParallelWriter ActuallyChangedIDs;

			public unsafe void Execute(int startIndex, int count)
			{
				TransformRendererState* ptr = stackalloc TransformRendererState[32];
				int num = 0;
				int* ptr2 = stackalloc int[32];
				int num2 = 0;
				for (int i = 0; i < count; i++)
				{
					int num3 = startIndex + i;
					int num4 = InstanceIDs[num3];
					GPUDrivenInstanceID gPUDrivenInstanceID = GPUDrivenInstanceID.UnityObject(num4);
					if (InstanceIDToRendererState.TryGetValue(gPUDrivenInstanceID, out var item) && item.IsEnabled)
					{
						ptr[num++] = new TransformRendererState
						{
							OriginalIndex = num3,
							InstanceID = gPUDrivenInstanceID,
							RendererState = item
						};
						ptr2[num2++] = num4;
					}
				}
				if (num > 0)
				{
					Result.AddRangeNoResize(ptr, num);
				}
				if (num2 > 0)
				{
					ActuallyChangedIDs.AddRangeNoResize(ptr2, num2);
				}
			}
		}

		[BurstCompile]
		public struct CheckChangedGroups : IJobParallelFor
		{
			public const int kBatchSize = 32;

			[ReadOnly]
			public NativeArray<TransformRendererState> RendererStates;

			[ReadOnly]
			public NativeHashMap<InstanceKey, InstanceData> InstanceKeyToInstanceData;

			[ReadOnly]
			[NativeDisableContainerSafetyRestriction]
			public NativeArray<TransformNativeData> NativeData;

			[ReadOnly]
			[NativeDisableContainerSafetyRestriction]
			public NativeArray<float4x4> LocalToWorldMatrices;

			[ReadOnly]
			public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

			public NativeList<RendererToMigrateData>.ParallelWriter RenderersToMigrate;

			public void Execute(int index)
			{
				TransformRendererState transformRendererState = RendererStates[index];
				bool flag = GPUDrivenRendererGroupPool.RendererSettings.ShouldFlipWinding(LocalToWorldMatrices[transformRendererState.OriginalIndex]);
				ref RendererState rendererState = ref transformRendererState.RendererState;
				for (int i = 0; i < rendererState.SubmeshCount; i++)
				{
					InstanceKey instanceKey = default(InstanceKey);
					instanceKey.InstanceID = transformRendererState.InstanceID;
					instanceKey.SubmeshIndex = i;
					InstanceKey instanceKey2 = instanceKey;
					if (InstanceKeyToInstanceData.TryGetValue(instanceKey2, out var item))
					{
						GPUDrivenRendererGroupPool.RendererGroup rendererGroup = RendererGroups[item.RendererGroupIndex];
						ref GPUDrivenRendererGroupPool.RendererSettings rendererSettings = ref rendererGroup.Key.RendererSettings;
						GPUDrivenRendererGroupPool.RendererSettings rendererSettings2 = rendererSettings;
						if (NativeData.IsCreated)
						{
							rendererSettings2.Scene = NativeData[transformRendererState.OriginalIndex].SceneHandle;
						}
						else
						{
							rendererSettings2.Scene = rendererSettings.Scene;
						}
						if (flag)
						{
							rendererSettings2.Flags |= GPUDrivenRendererGroupPool.RendererGroupFlags.FlipWinding;
						}
						else
						{
							rendererSettings2.Flags &= ~GPUDrivenRendererGroupPool.RendererGroupFlags.FlipWinding;
						}
						GPUDrivenSceneHandle scene = rendererSettings.Scene;
						if (!scene.Equals(rendererSettings2.Scene) || rendererSettings.Flags != rendererSettings2.Flags)
						{
							GPUDrivenRendererGroupPool.RendererGroupKey key = rendererGroup.Key;
							key.RendererSettings = rendererSettings2;
							RenderersToMigrate.AddNoResize(new RendererToMigrateData
							{
								InstanceKey = instanceKey2,
								NewGroupKey = key,
								InstanceData = item,
								SubmeshIndex = i,
								RendererStateIndex = index
							});
						}
					}
				}
			}
		}

		public struct RendererToMigrateData
		{
			public InstanceKey InstanceKey;

			public GPUDrivenRendererGroupPool.RendererGroupKey NewGroupKey;

			public InstanceData InstanceData;

			public int RendererStateIndex;

			public int SubmeshIndex;
		}

		public static NativeList<TransformNativeData> CollectNativeData(NativeArray<int> instanceIDs, Allocator allocator)
		{
			if (Application.isPlaying)
			{
				return default(NativeList<TransformNativeData>);
			}
			using (new ProfilingScope(Profiling.UpdateTransformJobsCollectNativeData))
			{
				NativeList<TransformNativeData> result = new NativeList<TransformNativeData>(instanceIDs.Length, allocator);
				foreach (int item in instanceIDs)
				{
					Renderer renderer = ObjectDispatcherService.FindByInstanceId<Renderer>(item);
					TransformNativeData value = default(TransformNativeData);
					Scene scene = renderer.gameObject.scene;
					value.SceneHandle = GPUDrivenSceneHandle.FromScene(in scene);
					result.Add(in value);
				}
				return result;
			}
		}

		public static void MigrateRendererGroup(GPUDrivenBatchRendererGroup brg, in TransformRendererState transformRendererState, in RendererToMigrateData rendererToMigrateData)
		{
			GameObject gameObject = ObjectDispatcherService.FindByInstanceId<MeshRenderer>(transformRendererState.InstanceID.RawInstanceID).gameObject;
			InstanceData instanceData = rendererToMigrateData.InstanceData;
			ref InstanceMetadata reference = ref UnsafeCollectionExtensions.ElementAsRef(in brg.m_InstanceMetadata, instanceData.IndexAllocation.Index);
			reference.RendererInstanceID = transformRendererState.InstanceID;
			reference.SubmeshIndex = rendererToMigrateData.SubmeshIndex;
			reference.GameObjectInstanceID = gameObject.GetInstanceID();
			Scene scene = gameObject.scene;
			reference.SceneIndex = brg.GetSceneIndex(GPUDrivenSceneHandle.FromScene(in scene));
			GPUDrivenRendererGroupPool.RendererGroupKey groupKey = rendererToMigrateData.NewGroupKey;
			brg.TryRegisterLightmappingMaterial(in groupKey);
			if (brg.RendererGroupPool.TryMigrateInstanceSimple(ref instanceData, in groupKey))
			{
				brg.m_InstanceKeyToInstanceData[rendererToMigrateData.InstanceKey] = instanceData;
			}
		}
	}

	public enum RendererUpdateStatus
	{
		CouldNotRegister,
		DidNotUpdateRegistered,
		UpdatedRegistered,
		Removed,
		RemovedAsUnsupported,
		Added
	}

	[BurstCompile]
	private struct CollectInstancesForNativeDataUpdateJob : IJobParallelForBatch
	{
		public const int kBatchSize = 32;

		public GPUDrivenInstanceID.InstanceIDType ObjectType;

		public ChangeContext ChangeContext;

		[NativeDisableContainerSafetyRestriction]
		[NativeDisableUnsafePtrRestriction]
		public GPUDrivenPropertyDataWriter PropertyDataWriter;

		[ReadOnly]
		public NativeArray<int>.ReadOnly RendererIDs;

		[ReadOnly]
		public NativeHashMap<InstanceKey, InstanceData>.ReadOnly InstanceKeyToInstanceData;

		[ReadOnly]
		public NativeArray<GPUDrivenAllocator.DataAllocation>.ReadOnly InstanceDataAllocations;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

		public NativeList<GPUDrivenNativeDataUpdate.Instance>.ParallelWriter UpdatedInstances;

		public NativeHashMap<GPUDrivenInstanceID, RendererState> InstanceIDToRendererState;

		public unsafe void Execute(int startIndex, int count)
		{
			GPUDrivenPropertyDataWriter propertyDataWriter = PropertyDataWriter;
			NativeList<GPUDrivenNativeDataUpdate.Instance> list = new NativeList<GPUDrivenNativeDataUpdate.Instance>(count * 8, Allocator.Temp);
			for (int i = startIndex; i < startIndex + count; i++)
			{
				GPUDrivenInstanceID gPUDrivenInstanceID = new GPUDrivenInstanceID(RendererIDs[i], ObjectType);
				if (!InstanceIDToRendererState.TryGetValue(gPUDrivenInstanceID, out var item) || ChangeContext.FrameIndex <= item.LastDefaultPerInstanceDataChangeFrameIndex)
				{
					continue;
				}
				item.LastDefaultPerInstanceDataChangeFrameIndex = ChangeContext.FrameIndex;
				InstanceIDToRendererState[gPUDrivenInstanceID] = item;
				for (int j = 0; j < item.SubmeshCount; j++)
				{
					InstanceKey instanceKey = default(InstanceKey);
					instanceKey.InstanceID = gPUDrivenInstanceID;
					instanceKey.SubmeshIndex = j;
					InstanceKey key = instanceKey;
					if (!InstanceKeyToInstanceData.TryGetValue(key, out var item2))
					{
						continue;
					}
					GPUDrivenNativeDataUpdate.Instance instance = default(GPUDrivenNativeDataUpdate.Instance);
					instance.IndexAllocation = item2.IndexAllocation;
					instance.RendererGroupIndex = item2.RendererGroupIndex;
					instance.NativeDataIndex = i;
					instance.RendererInstanceID = key.InstanceID;
					GPUDrivenNativeDataUpdate.Instance value = instance;
					ref readonly GPUDrivenRendererGroupPool.RendererSettings rendererSettings = ref UnsafeCollectionExtensions.ElementAsRef(in RendererGroups, item2.RendererGroupIndex).Key.RendererSettings;
					propertyDataWriter.ResetOffset();
					GPUDrivenAllocator.DataAllocation dataAllocation = InstanceDataAllocations[item2.IndexAllocation.Index];
					fixed (GPUDrivenMetadataAuthoring.DefaultPerInstanceData* defaultPerInstanceDataPtr = &propertyDataWriter.GetPropertyDataRawAndMove<GPUDrivenMetadataAuthoring.DefaultPerInstanceData>(in dataAllocation))
					{
						value.DefaultPerInstanceDataPtr = defaultPerInstanceDataPtr;
					}
					if ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightMaps) != 0)
					{
						fixed (GPUDrivenMetadataAuthoring.LightMapsPerInstanceData* lightMapsPerInstanceData = &propertyDataWriter.GetPropertyDataRawAndMove<GPUDrivenMetadataAuthoring.LightMapsPerInstanceData>(in dataAllocation))
						{
							value.LightMapsPerInstanceData = lightMapsPerInstanceData;
						}
					}
					if ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightProbes) != 0)
					{
						fixed (GPUDrivenMetadataAuthoring.LightProbesPerInstanceData* lightProbesPerInstanceData = &propertyDataWriter.GetPropertyDataRawAndMove<GPUDrivenMetadataAuthoring.LightProbesPerInstanceData>(in dataAllocation))
						{
							value.LightProbesPerInstanceData = lightProbesPerInstanceData;
						}
					}
					list.Add(in value);
				}
			}
			if (list.Length > 0)
			{
				UpdatedInstances.AddRangeNoResize(list);
			}
		}
	}

	public readonly struct RendererDesc
	{
		[CanBeNull]
		public readonly MeshRenderer MeshRenderer;

		public readonly GPUDrivenInstanceID InstanceID;

		[CanBeNull]
		public readonly MeshFilter MeshFilter;

		public readonly GPUDrivenVisibilityFlags ExtraVisibilityFlags;

		private RendererDesc([CanBeNull] MeshRenderer meshRenderer, GPUDrivenInstanceID instanceID, [CanBeNull] MeshFilter meshFilter = null, GPUDrivenVisibilityFlags extraVisibilityFlags = GPUDrivenVisibilityFlags.None)
		{
			MeshRenderer = meshRenderer;
			InstanceID = instanceID;
			MeshFilter = meshFilter;
			ExtraVisibilityFlags = extraVisibilityFlags;
		}

		public static RendererDesc FromMeshRenderer(MeshRenderer meshRenderer, [CanBeNull] MeshFilter meshFilter = null)
		{
			return new RendererDesc(meshRenderer, GPUDrivenInstanceID.UnityObject(meshRenderer.GetInstanceID()), meshFilter);
		}

		public static RendererDesc Custom(int instanceID, GPUDrivenVisibilityFlags extraVisibilityFlags = GPUDrivenVisibilityFlags.None)
		{
			return new RendererDesc(null, GPUDrivenInstanceID.Custom(instanceID), null, extraVisibilityFlags);
		}

		public static RendererDesc Custom(GPUDrivenInstanceID instanceID, GPUDrivenVisibilityFlags extraVisibilityFlags = GPUDrivenVisibilityFlags.None)
		{
			return new RendererDesc(null, instanceID, null, extraVisibilityFlags);
		}

		public override string ToString()
		{
			if (MeshRenderer != null)
			{
				return MeshRenderer.name;
			}
			return InstanceID.ToString();
		}
	}

	public struct RendererParams
	{
		[CanBeNull]
		public List<Material> Materials;

		[CanBeNull]
		public Mesh Mesh;

		public NativeArray<GPUDrivenRenderer.PropertyData> PerInstanceProperties;

		public bool Enabled;

		public int GameObjectInstanceID;

		public Scene Scene;

		public float4x4 LocalToWorldMatrix;

		public Vector3 TransformScale;

		public GeneralRendererSettings General;

		public ulong SceneCullingMask;
	}

	public struct GeneralRendererSettings
	{
		public int Layer;

		public uint RenderingLayerMask;

		public int SortingOrder;

		public int LightmapIndex;

		public MotionVectorGenerationMode MotionVectorGenerationMode;

		public ShadowCastingMode ShadowCastingMode;

		public bool StaticShadowCaster;

		public bool ReceiveShadows;

		public float4 LightmapScaleOffset;

		public Bounds OccluderBounds;

		public LightProbeUsage LightProbeUsage;
	}

	private struct RendererState
	{
		public int SubmeshCount;

		public bool IsEnabled;

		public int LastRendererDataChangeFrameIndex;

		public int LastDefaultPerInstanceDataChangeFrameIndex;

		public int LastCustomPerInstanceDataChangeFrameIndex;

		public int LastMeshChangeFrameIndex;

		public int LastMaterialDataChangeFrameIndex;

		public static RendererState Empty
		{
			get
			{
				RendererState result = default(RendererState);
				result.SubmeshCount = 0;
				result.LastRendererDataChangeFrameIndex = -1;
				result.LastDefaultPerInstanceDataChangeFrameIndex = -1;
				result.LastCustomPerInstanceDataChangeFrameIndex = -1;
				result.LastMeshChangeFrameIndex = -1;
				result.LastMaterialDataChangeFrameIndex = -1;
				return result;
			}
		}
	}

	public struct ChangeContext
	{
		public int FrameIndex;
	}

	public struct InstanceMetadata
	{
		public GPUDrivenInstanceID RendererInstanceID;

		public int SubmeshIndex;

		public int GameObjectInstanceID;

		public GPUDrivenIndexAllocator.IndexAllocation SceneIndex;

		public ulong SerialNumber;

		public int SortingOrder;
	}

	public struct InstanceData : IEquatable<InstanceData>
	{
		public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;

		public int RendererGroupIndex;

		public int InstanceSize;

		public bool Equals(InstanceData other)
		{
			if (IndexAllocation.Equals(other.IndexAllocation) && RendererGroupIndex == other.RendererGroupIndex)
			{
				return InstanceSize == other.InstanceSize;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is InstanceData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(IndexAllocation, RendererGroupIndex, InstanceSize);
		}
	}

	public struct InstanceKey : IEquatable<InstanceKey>
	{
		public GPUDrivenInstanceID InstanceID;

		public int SubmeshIndex;

		public bool Equals(InstanceKey other)
		{
			if (InstanceID.Equals(other.InstanceID))
			{
				return SubmeshIndex == other.SubmeshIndex;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is InstanceKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (InstanceID.GetHashCode() * 397) ^ SubmeshIndex;
		}

		public override string ToString()
		{
			return $"{InstanceID}:{SubmeshIndex}";
		}
	}

	private static class NativeDataUpdateValidator
	{
		public static void ValidateUnregisteredRenderers(GPUDrivenBatchRendererGroup brg, NativeArray<int> changedRendererIDs)
		{
		}

		public static void ValidateInvalidRenderers(GPUDrivenProcessor.NativeRenderersData nativeRenderersData)
		{
			using (new ProfilingScope(Profiling.UpdateNativeDataValidation))
			{
				List<UnityEngine.Object> value;
				using (ListPool<UnityEngine.Object>.Get(out value))
				{
					UnityEngine.Resources.InstanceIDToObjectList(nativeRenderersData.InvalidRendererIDs, value);
					foreach (MeshRenderer item in value)
					{
						RendererUtils.SetAllowGPUDrivenRendering(item, allow: false);
					}
				}
			}
		}
	}

	private const int kInvalidTetrahedronCacheIndex = -1;

	[CanBeNull]
	private static GPUDrivenBatchRendererGroup s_Instance;

	private readonly Material m_ErrorMaterial;

	private readonly GPUDrivenIndexAllocator m_InstanceIndexAllocator;

	private readonly Material m_PickingMaterial;

	private readonly GPUDrivenRendererGroupPool.RendererSettingsCreationInfo m_RendererSettingsCreationInfo;

	private readonly GPUDrivenBRGSettings m_Settings;

	private readonly Camera[] m_TempCameras = new Camera[1];

	private readonly List<Material> m_TempMaterialList = new List<Material>();

	private GPUDrivenBatchedDataUploader m_BatchedDataUploader;

	private SphericalHarmonicsL2 m_CachedAmbientProbe;

	private GPUDrivenBRGComponentMapping m_ComponentMapping;

	private int m_CurrentCullingSplitsCapacity;

	private GPUDrivenProcessor m_GPUDrivenProcessor;

	private bool m_Initialized;

	private NativeHashMap<GPUDrivenInstanceID, RendererState> m_InstanceIDToRendererState;

	private NativeHashMap<InstanceKey, InstanceData> m_InstanceKeyToInstanceData;

	private NativeArray<InstanceMetadata> m_InstanceMetadata;

	private NativeList<int> m_InstancesCreatedThisFrame;

	private ulong m_InstanceSerialNumber;

	private NativeHashSet<int> m_LastFrameMovingRendererIDs;

	private NativeHashSet<int> m_MovingRendererIDs;

	private GPUDrivenBRGObjectTracker m_ObjectTracker;

	private GPUDrivenPersistentSceneData m_PersistentSceneData;

	private NativeArray<int> m_TetrahedronCacheIndices;

	public GPUDrivenLODGroupRepository LODGroupRepository { get; private set; }

	public GPUDrivenBufferUtils BufferUtils { get; }

	public GPUDrivenCommandQueue CommandQueue { get; }

	public GPUDrivenPersistentData PersistentData { get; }

	public GPUDrivenBRGInstanceCuller InstanceCuller { get; private set; }

	public GPUDrivenCullingPassSharedData SharedPassData { get; } = new GPUDrivenCullingPassSharedData();


	public int InstanceCapacity => m_InstanceIndexAllocator.Capacity;

	public GPUDrivenCullingResourcesPool CullingResourcesPool => InstanceCuller.CullingResourcesPool;

	public GPUDrivenVisibilityMaskPool VisibilityMaskPool => InstanceCuller.VisibilityMaskPool;

	public GPUDrivenShaderMetadataRepository ShaderMetadataRepository { get; }

	public GPUDrivenRendererGroupPool RendererGroupPool { get; private set; }

	[CanBeNull]
	internal GPUDrivenBRGDebug DebugData { get; }

	internal GPUDrivenResourceRegistry ResourceRegistry { get; private set; }

	public ref readonly GPUDrivenBRGSettings Settings => ref m_Settings;

	public bool IsEnabledAndInitialized
	{
		get
		{
			if (Settings.IsEnabledAndSupported)
			{
				return m_Initialized;
			}
			return false;
		}
	}

	public PipelineRuntimeResources Resources { get; }

	public GPUDrivenLightmapping Lightmapping { get; private set; }

	public NativeHashMap<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation>.ReadOnly ActiveRendererGroups => RendererGroupPool.ActiveRendererGroups;

	public GraphicsBuffer PersistentIndicesBuffer => RendererGroupPool.PersistentIndicesBuffer.InternalBuffer;

	public GraphicsBuffer GroupInfoBuffer => RendererGroupPool.GroupInfoBuffer.InternalBuffer;

	public GraphicsBuffer GPUCullingJobsBuffer => RendererGroupPool.GPUCullingJobsBuffer.InternalBuffer;

	public GraphicsBuffer VisibilityInfoBuffer => PersistentData.VisibilityInfoBuffer.InternalBuffer;

	internal BatchRendererGroup BRG { get; private set; }

	[CanBeNull]
	public Camera OverrideCamera
	{
		get
		{
			GPUDrivenBRGDebug debugData = DebugData;
			if (debugData == null || !debugData.OverrideCameraToMain)
			{
				return null;
			}
			return GetMainCameraOrDefault();
		}
	}

	public int InstancesCount => m_InstanceKeyToInstanceData.Count;

	public static event Action<GPUDrivenBatchRendererGroup> Created;

	public static event Action<GPUDrivenBatchRendererGroup> Disposing;

	public event Action<NativeArray<int>.ReadOnly> OnCreatedMaterials;

	public void BatchUpdateRendererTransforms(NativeArray<int> instanceIDs, NativeArray<Matrix4x4> localToWorldMatrices, out NativeList<int> actuallyChangedIDs)
	{
		using (new ProfilingScope(Profiling.BatchUpdateRendererTransforms))
		{
			NativeList<UpdateTransformJobs.TransformRendererState> nativeArray = new NativeList<UpdateTransformJobs.TransformRendererState>(instanceIDs.Length, Allocator.TempJob);
			NativeList<int> nativeList = new NativeList<int>(instanceIDs.Length, Allocator.TempJob);
			UpdateTransformJobs.CollectRendererStatesJob jobData = default(UpdateTransformJobs.CollectRendererStatesJob);
			jobData.InstanceIDToRendererState = m_InstanceIDToRendererState;
			jobData.InstanceIDs = instanceIDs;
			jobData.Result = nativeArray.AsParallelWriter();
			jobData.ActuallyChangedIDs = nativeList.AsParallelWriter();
			JobHandle jobHandle = IJobParallelForBatchExtensions.Schedule(jobData, instanceIDs.Length, 32);
			JobHandle.ScheduleBatchedJobs();
			NativeList<UpdateTransformJobs.TransformNativeData> nativeList2 = UpdateTransformJobs.CollectNativeData(instanceIDs, Allocator.TempJob);
			jobHandle.Complete();
			NativeList<UpdateTransformJobs.RendererToMigrateData> nativeList3 = new NativeList<UpdateTransformJobs.RendererToMigrateData>(nativeArray.Length * 64, Allocator.TempJob);
			UpdateTransformJobs.CheckChangedGroups jobData2 = default(UpdateTransformJobs.CheckChangedGroups);
			jobData2.RendererGroups = RendererGroupPool.GetInnerPool();
			jobData2.RendererStates = nativeArray.AsArray();
			jobData2.NativeData = (nativeList2.IsCreated ? nativeList2.AsArray() : default(NativeArray<UpdateTransformJobs.TransformNativeData>));
			jobData2.LocalToWorldMatrices = localToWorldMatrices.Reinterpret<float4x4>();
			jobData2.InstanceKeyToInstanceData = m_InstanceKeyToInstanceData;
			jobData2.RenderersToMigrate = nativeList3.AsParallelWriter();
			IJobParallelForExtensions.Schedule(jobData2, nativeArray.Length, 32).Complete();
			foreach (UpdateTransformJobs.RendererToMigrateData item in nativeList3)
			{
				UpdateTransformJobs.RendererToMigrateData rendererToMigrateData = item;
				UpdateTransformJobs.MigrateRendererGroup(this, in UnsafeCollectionExtensions.ElementAsRef(in nativeArray, rendererToMigrateData.RendererStateIndex), in rendererToMigrateData);
			}
			nativeArray.Dispose();
			if (nativeList2.IsCreated)
			{
				nativeList2.Dispose();
			}
			nativeList3.Dispose();
			actuallyChangedIDs = nativeList;
		}
	}

	public GPUDrivenBatchRendererGroup(GPUDrivenBRGSettings settings, PipelineRuntimeResources resources, GPUDrivenCommandQueue commandQueue, GPUDrivenBufferUtils bufferUtils, [CanBeNull] GPUDrivenBRGDebug debugData, [CanBeNull] WaaaghDebugData waaaghDebugData)
	{
		m_Settings = settings;
		Resources = resources;
		DebugData = debugData;
		if (m_Settings.IsEnabledAndSupported)
		{
			m_ErrorMaterial = CoreUtils.CreateEngineMaterial(resources.DOTSBRGErrorShaderPS);
			m_PickingMaterial = CoreUtils.CreateEngineMaterial(resources.BRGPickingPS);
			BRG = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);
			BRG.SetGlobalBounds(new Bounds(Vector3.zero, Vector3.one * 1000000f));
			BRG.SetErrorMaterial(m_ErrorMaterial);
			BRG.SetPickingMaterial(m_PickingMaterial);
			BRG.SetEnabledViewTypes(new BatchCullingViewType[5]
			{
				BatchCullingViewType.Camera,
				BatchCullingViewType.Light,
				BatchCullingViewType.Picking,
				BatchCullingViewType.SelectionOutline,
				BatchCullingViewType.Filtering
			});
			BufferUtils = bufferUtils;
			CommandQueue = commandQueue;
			m_BatchedDataUploader = new GPUDrivenBatchedDataUploader(resources, settings.UploadMode);
			m_GPUDrivenProcessor = new GPUDrivenProcessor();
			LODGroupRepository = new GPUDrivenLODGroupRepository(this, settings, m_GPUDrivenProcessor, m_BatchedDataUploader);
			m_InstanceIndexAllocator = new GPUDrivenIndexAllocator(settings.InitialInstanceCapacity);
			m_InstanceKeyToInstanceData = new NativeHashMap<InstanceKey, InstanceData>(settings.InitialInstanceCapacity, Allocator.Persistent);
			m_InstanceIDToRendererState = new NativeHashMap<GPUDrivenInstanceID, RendererState>(settings.InitialInstanceCapacity, Allocator.Persistent);
			m_ComponentMapping = new GPUDrivenBRGComponentMapping(settings.InitialInstanceCapacity, Allocator.Persistent);
			m_InstanceMetadata = new NativeArray<InstanceMetadata>(settings.InitialInstanceCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			PersistentData = new GPUDrivenPersistentData(this, m_BatchedDataUploader, LODGroupRepository);
			ShaderMetadataRepository = new GPUDrivenShaderMetadataRepository();
			Lightmapping = new GPUDrivenLightmapping(BRG);
			m_PersistentSceneData = new GPUDrivenPersistentSceneData(settings, Lightmapping);
			ResourceRegistry = new GPUDrivenResourceRegistry(this, PersistentData, Lightmapping);
			m_ObjectTracker = new GPUDrivenBRGObjectTracker(this);
			RendererGroupPool = new GPUDrivenRendererGroupPool(this, ResourceRegistry, waaaghDebugData);
			m_RendererSettingsCreationInfo = new GPUDrivenRendererGroupPool.RendererSettingsCreationInfo
			{
				BRGSettings = settings,
				LODCrossFadeEnabled = WaaaghPipeline.Asset.DitheringSettings.EnableLODCrossFade
			};
			InstanceCuller = new GPUDrivenBRGInstanceCuller(this, ResourceRegistry, RendererGroupPool, PersistentData, Lightmapping, BufferUtils, CommandQueue, LODGroupRepository, waaaghDebugData);
			m_TetrahedronCacheIndices = new NativeArray<int>(settings.InitialInstanceCapacity, Allocator.Persistent);
			ref NativeArray<int> tetrahedronCacheIndices = ref m_TetrahedronCacheIndices;
			int value = -1;
			tetrahedronCacheIndices.FillArray(in value);
			m_MovingRendererIDs = new NativeHashSet<int>(256, Allocator.Persistent);
			m_LastFrameMovingRendererIDs = new NativeHashSet<int>(256, Allocator.Persistent);
			m_InstancesCreatedThisFrame = new NativeList<int>(settings.InitialInstanceCapacity, Allocator.Persistent);
			ObjectDispatcherService.OnUpdated += OnObjectDispatcherUpdated;
			s_Instance = this;
			m_Initialized = true;
			GPUDrivenBatchRendererGroup.Created?.Invoke(this);
		}
	}

	public void Dispose()
	{
		SharedPassData?.Dispose();
		if (!m_Initialized)
		{
			return;
		}
		m_Initialized = false;
		GPUDrivenBatchRendererGroup.Disposing?.Invoke(this);
		s_Instance = null;
		ObjectDispatcherService.OnUpdated -= OnObjectDispatcherUpdated;
		if (m_GPUDrivenProcessor != null)
		{
			if (m_InstanceIDToRendererState.Count > 0)
			{
				NativeList<int> nativeList = new NativeList<int>(m_InstanceKeyToInstanceData.Count, Allocator.Temp);
				foreach (KVPair<InstanceKey, InstanceData> instanceKeyToInstanceDatum in m_InstanceKeyToInstanceData)
				{
					GPUDrivenInstanceID instanceID = instanceKeyToInstanceDatum.Key.InstanceID;
					if (instanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject)
					{
						nativeList.Add(in instanceID.RawInstanceID);
					}
				}
				GPUDrivenProcessor gPUDrivenProcessor = m_GPUDrivenProcessor;
				NativeArray<int> source = nativeList.AsArray();
				gPUDrivenProcessor.DisableGPUDrivenRendering(source);
				nativeList.Dispose();
			}
			m_GPUDrivenProcessor.Dispose();
			m_GPUDrivenProcessor = null;
		}
		if (LODGroupRepository != null)
		{
			LODGroupRepository.Dispose();
			LODGroupRepository = null;
		}
		if (m_ErrorMaterial != null)
		{
			CoreUtils.Destroy(m_ErrorMaterial);
		}
		if (m_PickingMaterial != null)
		{
			CoreUtils.Destroy(m_PickingMaterial);
		}
		if (InstanceCuller != null)
		{
			InstanceCuller.Dispose();
			InstanceCuller = null;
		}
		PersistentData?.Dispose();
		DisposeCollectionIfCreated(ref m_InstanceKeyToInstanceData);
		DisposeCollectionIfCreated(ref m_InstanceIDToRendererState);
		DisposeCollectionIfCreated(ref m_InstanceMetadata);
		DisposeCollectionIfCreated(ref m_InstancesCreatedThisFrame);
		m_InstanceIndexAllocator?.Dispose();
		m_ComponentMapping.Dispose();
		if (RendererGroupPool != null)
		{
			RendererGroupPool.Dispose();
			RendererGroupPool = null;
		}
		if (m_PersistentSceneData != null)
		{
			m_PersistentSceneData.Dispose();
			m_PersistentSceneData = null;
		}
		if (Lightmapping != null)
		{
			Lightmapping.Dispose();
			Lightmapping = null;
		}
		if (ResourceRegistry != null)
		{
			ResourceRegistry.Dispose();
			ResourceRegistry = null;
		}
		if (m_ObjectTracker != null)
		{
			m_ObjectTracker.Dispose();
			m_ObjectTracker = null;
		}
		if (BRG != null)
		{
			BRG.Dispose();
			BRG = null;
		}
		if (m_BatchedDataUploader != null)
		{
			m_BatchedDataUploader.Dispose();
			m_BatchedDataUploader = null;
		}
		DisposeCollectionIfCreated(ref m_TetrahedronCacheIndices);
		DisposeCollectionIfCreated(ref m_MovingRendererIDs);
		DisposeCollectionIfCreated(ref m_LastFrameMovingRendererIDs);
		ShaderMetadataRepository?.Dispose();
		SharedPassData?.Dispose();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.InstanceDataCPU, m_InstanceMetadata);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_InstanceIDToRendererState);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_InstanceKeyToInstanceData);
		counters.CollectBufferSize(counters.MiscCPU, m_TetrahedronCacheIndices);
		counters.CollectBufferSize(counters.MiscCPU, m_InstancesCreatedThisFrame);
		m_ComponentMapping.FillMemoryCounters(counters);
		m_InstanceIndexAllocator.FillMemoryCounter(counters, counters.InstanceDataCPU);
		m_PersistentSceneData.FillMemoryCounters(counters);
		PersistentData.FillMemoryCounters(counters);
		InstanceCuller.FillMemoryCounters(counters);
		RendererGroupPool.FillMemoryCounters(counters);
		ResourceRegistry.FillMemoryCounters(counters);
		LODGroupRepository.FillMemoryCounters(counters);
		counters.ComputeTotal();
	}

	public static bool TryGetInstance(out GPUDrivenBatchRendererGroup instance)
	{
		instance = s_Instance;
		return instance != null;
	}

	private void OnObjectDispatcherUpdated()
	{
		UpdateAmbientProbe();
	}

	public NativeArray<int> GetInstancesCreatedThisFrame()
	{
		return m_InstancesCreatedThisFrame.AsArray();
	}

	private void GrowInstanceCount()
	{
		int capacity = m_InstanceIndexAllocator.Capacity;
		m_InstanceIndexAllocator.ForceGrow(capacity * 2);
		int capacity2 = m_InstanceIndexAllocator.Capacity;
		ArrayExtensions.ResizeArray(ref m_InstanceMetadata, capacity2);
		ArrayExtensions.ResizeArray(ref m_TetrahedronCacheIndices, capacity2);
		ref NativeArray<int> tetrahedronCacheIndices = ref m_TetrahedronCacheIndices;
		int value = -1;
		tetrahedronCacheIndices.FillArray(in value, capacity);
		RendererGroupPool.ResizeInstanceCount(capacity2);
		PersistentData.GrowInstanceCount(capacity2);
	}

	private Camera GetMainCameraOrDefault()
	{
		Camera main = Camera.main;
		if (main != null)
		{
			return main;
		}
		Camera.GetAllCameras(m_TempCameras);
		main = m_TempCameras[0];
		m_TempCameras[0] = null;
		return main;
	}

	public ref readonly UnsafeList<int> ReadRendererGroupIndices(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref RendererGroupPool.ReadIndices(indexAllocation);
	}

	private static void DisposeCollectionIfCreated<T>(ref NativeArray<T> array) where T : struct
	{
		if (array.IsCreated)
		{
			array.Dispose();
			array = default(NativeArray<T>);
		}
	}

	private static void DisposeCollectionIfCreated<T>(ref NativeList<T> list) where T : unmanaged
	{
		if (list.IsCreated)
		{
			list.Dispose();
			list = default(NativeList<T>);
		}
	}

	private static void DisposeCollectionIfCreated<T>(ref NativeHashSet<T> hashSet) where T : unmanaged, IEquatable<T>
	{
		if (hashSet.IsCreated)
		{
			hashSet.Dispose();
			hashSet = default(NativeHashSet<T>);
		}
	}

	private static void DisposeCollectionIfCreated<TKey, TValue>(ref NativeHashMap<TKey, TValue> hashMap) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		if (hashMap.IsCreated)
		{
			hashMap.Dispose();
			hashMap = default(NativeHashMap<TKey, TValue>);
		}
	}

	public ref readonly InstanceMetadata ReadInstanceMetadata(int index)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_InstanceMetadata, index);
	}

	public NativeArray<InstanceMetadata>.ReadOnly GetAllInstanceMetadataReadonly()
	{
		return m_InstanceMetadata.AsReadOnly();
	}

	public NativeHashMap<InstanceKey, InstanceData>.ReadOnly GetInstanceKeyToDataReadonly()
	{
		return m_InstanceKeyToInstanceData.AsReadOnly();
	}

	public void WriteMaterialData(Material material, NativeArray<GPUDrivenRenderer.PropertyData> materialPropertyData)
	{
		GPUDrivenIndexAllocator.IndexAllocation orRegisterMaterial = ResourceRegistry.GetOrRegisterMaterial(material);
		if (orRegisterMaterial.Equals(GPUDrivenIndexAllocator.IndexAllocation.Invalid))
		{
			GPUDrivenErrorLogging.FailedToAllocateMaterial(material);
		}
		else
		{
			WriteMaterialData(orRegisterMaterial, materialPropertyData, revertDefaults: false);
		}
	}

	public void WriteMaterialData(GPUDrivenIndexAllocator.IndexAllocation indexAllocation, NativeArray<GPUDrivenRenderer.PropertyData> materialPropertyData)
	{
		WriteMaterialData(indexAllocation, materialPropertyData, revertDefaults: false);
	}

	public bool TryFindInstanceIndexAllocation(GPUDrivenInstanceID instanceID, int submeshIndex, out GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		if (m_InstanceKeyToInstanceData.TryGetValue(new InstanceKey
		{
			InstanceID = instanceID,
			SubmeshIndex = submeshIndex
		}, out var item))
		{
			indexAllocation = item.IndexAllocation;
			return true;
		}
		indexAllocation = GPUDrivenIndexAllocator.IndexAllocation.Invalid;
		return false;
	}

	public bool TryFindMaterialIndexAllocation(Material material, out GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ResourceRegistry.TryGetMaterialIndexAllocation(material, out indexAllocation);
	}

	public bool TryFindMaterialIndexAllocation(int materialInstanceID, out GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ResourceRegistry.TryGetMaterialIndexAllocation(materialInstanceID, out indexAllocation);
	}

	public void WriteDefaultMaterialData(Material material)
	{
		if (TryFindMaterialIndexAllocation(material, out var indexAllocation))
		{
			WriteDefaultMaterialData(indexAllocation);
		}
	}

	internal void WriteDefaultMaterialData(GPUDrivenIndexAllocator.IndexAllocation indexAllocation, NativeArray<GPUDrivenRenderer.PropertyData> propertiesToRevert = default(NativeArray<GPUDrivenRenderer.PropertyData>))
	{
		NativeArray<GPUDrivenRenderer.PropertyData> materialPropertyData = (propertiesToRevert.IsCreated ? propertiesToRevert : ResourceRegistry.GetManagedMaterialInfo(indexAllocation).PropertyLayout.PerMaterialData.AsArray());
		WriteMaterialData(indexAllocation, materialPropertyData, revertDefaults: true);
	}

	internal void ProcessShaderChanges(Shader shader, in ChangeContext changeContext)
	{
		NativeList<GPUDrivenIndexAllocator.IndexAllocation> drasticallyChangedMaterials = new NativeList<GPUDrivenIndexAllocator.IndexAllocation>(Allocator.Temp);
		bool flag = false;
		if (ResourceRegistry.MaterialPool.Count == 0)
		{
			return;
		}
		foreach (GPUDrivenRegisteredResource<GPUDrivenResourceRegistry.MaterialKey> item in ResourceRegistry.MaterialPool)
		{
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = item.IndexAllocation;
			ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(indexAllocation);
			if (managedMaterialInfo.OriginalMaterial != null && managedMaterialInfo.OriginalMaterial.shader == shader)
			{
				GPUDrivenRenderingUtils.MaterialLayoutChanges materialLayoutChanges = ProcessMaterialChanges(indexAllocation, flag, drasticallyChangedMaterials);
				if (flag || (materialLayoutChanges.PerMaterialData & GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges) != 0)
				{
					ResourceRegistry.UpdateMaterialUniquenessKey(indexAllocation);
					RendererGroupPool.OnModifiedCPUData();
				}
			}
		}
		if (drasticallyChangedMaterials.Length > 0)
		{
			ProcessDrasticallyChangedMaterials(drasticallyChangedMaterials.AsArray(), in changeContext);
		}
	}

	private void ProcessDrasticallyChangedMaterials(NativeArray<GPUDrivenIndexAllocator.IndexAllocation> drasticallyChangedMaterials, in ChangeContext changeContext)
	{
		foreach (GPUDrivenIndexAllocator.IndexAllocation item in drasticallyChangedMaterials)
		{
			ResourceRegistry.ReallocatePerMaterialData(item);
		}
		NativeList<GPUDrivenRendererGroupPool.RendererGroupKey> nativeList = new NativeList<GPUDrivenRendererGroupPool.RendererGroupKey>(Allocator.Temp);
		foreach (KVPair<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> activeRendererGroup in ActiveRendererGroups)
		{
			foreach (GPUDrivenIndexAllocator.IndexAllocation item2 in drasticallyChangedMaterials)
			{
				if (activeRendererGroup.Key.MaterialAllocation.Equals(item2))
				{
					GPUDrivenRendererGroupPool.RendererGroupKey value = activeRendererGroup.Key;
					nativeList.Add(in value);
				}
			}
		}
		if (nativeList.Length == 0)
		{
			return;
		}
		NativeList<int> nativeList2 = new NativeList<int>(Allocator.Temp);
		NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(nativeList2.Length, Allocator.Temp);
		foreach (GPUDrivenRendererGroupPool.RendererGroupKey item3 in nativeList)
		{
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = ActiveRendererGroups[item3];
			foreach (int item4 in RendererGroupPool.ReadIndices(indexAllocation))
			{
				GameObject gameObject = ObjectDispatcherService.FindByInstanceId<GameObject>(UnsafeCollectionExtensions.ElementAsRef(in m_InstanceMetadata, item4).GameObjectInstanceID);
				if (gameObject != null && gameObject.TryGetComponent<MeshRenderer>(out var component))
				{
					int value2 = component.GetInstanceID();
					if (!nativeHashSet.Contains(value2))
					{
						nativeHashSet.Add(value2);
						nativeList2.Add(in value2);
					}
				}
			}
		}
		if (nativeList2.Length == 0)
		{
			return;
		}
		foreach (int item5 in nativeList2)
		{
			RemoveRenderer(GPUDrivenInstanceID.UnityObject(item5));
		}
		NativeList<int> nativeList3 = new NativeList<int>(Allocator.Temp);
		int num = 0;
		while (num < nativeList2.Length)
		{
			int value3 = nativeList2[num];
			MeshRenderer meshRenderer = ObjectDispatcherService.FindByInstanceId<MeshRenderer>(value3);
			RendererDesc rendererDesc = RendererDesc.FromMeshRenderer(meshRenderer);
			RendererParams rendererParams = default(RendererParams);
			if (AddOrUpdateRenderer(in rendererDesc, in changeContext, GPUDrivenRendererGroupPool.RendererUpdateFlags.ForcedUpdate, in rendererParams) == RendererUpdateStatus.Added)
			{
				if (GPUDrivenRenderingUtils.IsRendererEnabled(meshRenderer))
				{
					num++;
				}
				else
				{
					nativeList2.RemoveAtSwapBack(num);
				}
			}
			else
			{
				nativeList2.RemoveAtSwapBack(num);
				nativeList3.Add(in value3);
			}
		}
		if (nativeList2.Length > 0)
		{
			EnableGPUDrivenRenderingAndLoadNativeData(nativeList2.AsArray(), in changeContext);
		}
		if (nativeList3.Length > 0)
		{
			DisableGPURendering(nativeList3.AsArray());
		}
	}

	private GPUDrivenRenderingUtils.MaterialLayoutChanges ProcessMaterialChanges(GPUDrivenIndexAllocator.IndexAllocation indexAllocation, bool shaderMetadataChanged, NativeList<GPUDrivenIndexAllocator.IndexAllocation> drasticallyChangedMaterials)
	{
		if (ResourceRegistry.TryUpdateMaterialFlags(indexAllocation))
		{
			RendererGroupPool.OnModifiedCPUData();
		}
		GPUDrivenRenderingUtils.MaterialLayoutChanges changes = UpdateMaterialPropertyLayout(indexAllocation);
		if (shaderMetadataChanged)
		{
			drasticallyChangedMaterials.Add(in indexAllocation);
			return changes;
		}
		if (!changes.Any())
		{
			return changes;
		}
		if (changes.AreDrastic())
		{
			drasticallyChangedMaterials.Add(in indexAllocation);
		}
		else
		{
			if ((changes.PerMaterialData & GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.DefaultValues) != 0)
			{
				WriteNonOverridenMaterialData(changes.DirtyPerMaterialDataMask, indexAllocation);
			}
			if ((changes.PerMaterialData & GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges) != 0)
			{
				ResourceRegistry.UpdateMaterialUniquenessKey(indexAllocation);
				RendererGroupPool.OnModifiedCPUData();
			}
		}
		return changes;
	}

	private GPUDrivenRenderingUtils.MaterialLayoutChanges UpdateMaterialPropertyLayout(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(indexAllocation);
		ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref ResourceRegistry.GetUnmanagedMaterialInfo(indexAllocation);
		return ResourceRegistry.UpdateMaterialLayout(ref managedMaterialInfo, in unmanagedMaterialInfo);
	}

	private void WriteNonOverridenMaterialData(BitMask256 dirtyPropertiesMask, GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(indexAllocation);
		ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref ResourceRegistry.GetUnmanagedMaterialInfo(indexAllocation);
		GPUDrivenPropertyDataWriter gPUDrivenPropertyDataWriter = PersistentData.BeginWritingProperties();
		for (int i = 0; i < managedMaterialInfo.PropertyLayout.PerMaterialData.Length; i++)
		{
			if (!dirtyPropertiesMask.Any())
			{
				break;
			}
			BitMask256 other = default(BitMask256);
			other.SetBit(i, value: true);
			GPUDrivenRenderer.PropertyData propertyData = managedMaterialInfo.PropertyLayout.PerMaterialData[i];
			if (!unmanagedMaterialInfo.PropertyOverrideMask.And(in other).Any() && dirtyPropertiesMask.And(in other).Any())
			{
				gPUDrivenPropertyDataWriter.WritePropertyData(in unmanagedMaterialInfo.MaterialDataAllocation, in propertyData);
			}
			else
			{
				gPUDrivenPropertyDataWriter.SkipPropertyData(in propertyData);
			}
			dirtyPropertiesMask.SetBit(i, value: false);
		}
	}

	private void WriteMaterialData(GPUDrivenIndexAllocator.IndexAllocation indexAllocation, NativeArray<GPUDrivenRenderer.PropertyData> materialPropertyData, bool revertDefaults)
	{
		ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(indexAllocation);
		ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref ResourceRegistry.GetUnmanagedMaterialInfo(indexAllocation);
		int length = managedMaterialInfo.PropertyLayout.PerMaterialData.Length;
		if (revertDefaults)
		{
			if (unmanagedMaterialInfo.PropertyOverrideMask.Equals(BitMask256.FirstBitsSet(length)) && materialPropertyData.Equals(managedMaterialInfo.PropertyLayout.PerMaterialData.AsArray()))
			{
				GPUDrivenPropertyDataWriter gPUDrivenPropertyDataWriter = PersistentData.BeginWritingProperties(autoMarkDirty: false);
				gPUDrivenPropertyDataWriter.MarkWholeAllocationDirty(in unmanagedMaterialInfo.MaterialDataAllocation);
				for (int i = 0; i < length; i++)
				{
					ref GPUDrivenRenderer.PropertyData propertyData = ref UnsafeCollectionExtensions.ElementAsRef(in managedMaterialInfo.PropertyLayout.PerMaterialData, i);
					gPUDrivenPropertyDataWriter.WritePropertyData(in unmanagedMaterialInfo.MaterialDataAllocation, in propertyData);
				}
				unmanagedMaterialInfo.PropertyOverrideMask = default(BitMask256);
			}
			else
			{
				GPUDrivenPropertyDataWriter gPUDrivenPropertyDataWriter2 = PersistentData.BeginWritingProperties();
				for (int j = 0; j < length; j++)
				{
					BitMask256 other = default(BitMask256);
					other.SetBit(j, value: true);
					ref GPUDrivenRenderer.PropertyData reference = ref UnsafeCollectionExtensions.ElementAsRef(in managedMaterialInfo.PropertyLayout.PerMaterialData, j);
					if (unmanagedMaterialInfo.PropertyOverrideMask.And(in other).Any())
					{
						bool flag = false;
						for (int k = 0; k < materialPropertyData.Length; k++)
						{
							if (UnsafeCollectionExtensions.ElementAsRef(in materialPropertyData, k).NameID == reference.NameID)
							{
								gPUDrivenPropertyDataWriter2.WritePropertyData(in unmanagedMaterialInfo.MaterialDataAllocation, in reference);
								unmanagedMaterialInfo.PropertyOverrideMask.SetBit(j, value: false);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							gPUDrivenPropertyDataWriter2.SkipPropertyData(in reference);
						}
					}
					else
					{
						gPUDrivenPropertyDataWriter2.SkipPropertyData(in reference);
					}
				}
			}
			ResourceRegistry.TryFreeMaterial(indexAllocation);
			return;
		}
		GPUDrivenPropertyDataWriter gPUDrivenPropertyDataWriter3 = PersistentData.BeginWritingProperties();
		for (int l = 0; l < length; l++)
		{
			GPUDrivenRenderer.PropertyData propertyData2 = managedMaterialInfo.PropertyLayout.PerMaterialData[l];
			bool flag2 = false;
			for (int m = 0; m < materialPropertyData.Length; m++)
			{
				GPUDrivenRenderer.PropertyData propertyData3 = materialPropertyData[m];
				if (propertyData3.NameID == propertyData2.NameID)
				{
					gPUDrivenPropertyDataWriter3.WritePropertyData(in unmanagedMaterialInfo.MaterialDataAllocation, in propertyData3);
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				unmanagedMaterialInfo.PropertyOverrideMask.SetBit(l, value: true);
			}
			else
			{
				gPUDrivenPropertyDataWriter3.SkipPropertyData(in propertyData2);
			}
		}
	}

	public RendererUpdateStatus AddOrUpdateRenderer(in RendererDesc rendererDesc, in ChangeContext changeContext, GPUDrivenRendererGroupPool.RendererUpdateFlags updateFlags, in RendererParams rendererParams = default(RendererParams))
	{
		using (new ProfilingScope(Profiling.AddOrUpdateRenderer))
		{
			GPUDrivenInstanceID instanceID = rendererDesc.InstanceID;
			if (!GPUDrivenRenderingUtils.IsRendererSupported(in rendererDesc))
			{
				return RemoveRenderer(instanceID, RendererUpdateStatus.RemovedAsUnsupported);
			}
			if (!m_InstanceIDToRendererState.TryGetValue(instanceID, out var item))
			{
				return AddRenderer(in rendererDesc, in rendererParams, in changeContext);
			}
			if ((updateFlags & GPUDrivenRendererGroupPool.RendererUpdateFlags.ForcedUpdate) != 0)
			{
				RemoveRenderer(instanceID);
				return AddRenderer(in rendererDesc, in rendererParams, in changeContext);
			}
			item.IsEnabled = rendererDesc.GetEnabled(in rendererParams);
			if ((updateFlags & GPUDrivenRendererGroupPool.RendererUpdateFlags.MaterialData) != 0 && !UpdateMaterialData(in rendererDesc, in rendererParams, in changeContext, ref item, ref updateFlags))
			{
				return RemoveRenderer(instanceID, RendererUpdateStatus.RemovedAsUnsupported);
			}
			if ((updateFlags & GPUDrivenRendererGroupPool.RendererUpdateFlags.RendererData) != 0)
			{
				UpdateRendererData(in rendererDesc, in rendererParams, in changeContext, ref item, ref updateFlags);
			}
			if ((updateFlags & GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData) != 0)
			{
				UpdateRendererCustomPerInstanceData(in rendererDesc, in rendererParams, in changeContext, ref item);
			}
			m_InstanceIDToRendererState[instanceID] = item;
			return RendererUpdateStatus.UpdatedRegistered;
		}
	}

	private RendererUpdateStatus AddRenderer(in RendererDesc rendererDesc, in RendererParams rendererParams, in ChangeContext changeContext)
	{
		if (!rendererDesc.TryGetMesh(in rendererParams, out var meshFilter, out var mesh) || mesh.vertexCount == 0 || mesh.subMeshCount == 0)
		{
			return RendererUpdateStatus.CouldNotRegister;
		}
		if (!GPUDrivenRenderingUtils.IsRendererSupported(in rendererDesc))
		{
			return RendererUpdateStatus.CouldNotRegister;
		}
		GPUDrivenInstanceID instanceID = rendererDesc.InstanceID;
		List<Material> tempMaterialList = m_TempMaterialList;
		rendererDesc.GetMaterials(in rendererParams, tempMaterialList);
		if (!AreMaterialsSupported(tempMaterialList))
		{
			tempMaterialList.Clear();
			return RendererUpdateStatus.CouldNotRegister;
		}
		if (meshFilter != null)
		{
			m_ComponentMapping.AddMeshFilterMapping(instanceID, GPUDrivenInstanceID.UnityObject(meshFilter.GetInstanceID()));
		}
		bool flag = false;
		GPUDrivenRendererGroupPool.RendererGroupKey groupKey = default(GPUDrivenRendererGroupPool.RendererGroupKey);
		for (int i = 0; i < tempMaterialList.Count; i++)
		{
			Material material = CorrectMaterial(tempMaterialList[i]);
			groupKey.MeshAllocation = ResourceRegistry.GetOrRegisterMesh(mesh, i);
			groupKey.MaterialAllocation = ResourceRegistry.GetOrRegisterMaterial(material);
			if (groupKey.MaterialAllocation.Equals(GPUDrivenIndexAllocator.IndexAllocation.Invalid))
			{
				flag = true;
				continue;
			}
			groupKey.RendererSettings = GPUDrivenRendererGroupPool.RendererSettings.From(in rendererDesc, in rendererParams, m_RendererSettingsCreationInfo);
			TryRegisterLightmappingMaterial(in groupKey);
			int orAllocate = RendererGroupPool.GetOrAllocate(in groupKey);
			if (orAllocate == -1)
			{
				flag = true;
				continue;
			}
			InstanceKey instanceKey = default(InstanceKey);
			instanceKey.InstanceID = instanceID;
			instanceKey.SubmeshIndex = i;
			InstanceKey instanceKey2 = instanceKey;
			if (!TryAllocateInstance(in rendererDesc, in rendererParams, instanceKey2, orAllocate, in changeContext, out var instanceSize))
			{
				flag = true;
			}
			else if (instanceSize > PersistentData.ReferenceInstanceSizeInBytes)
			{
				Debug.LogWarning($"Per Instance Data of {rendererDesc} has exceeded the limit: {instanceSize}/{PersistentData.ReferenceInstanceSizeInBytes}.", rendererDesc.MeshRenderer);
			}
		}
		tempMaterialList.Clear();
		if (flag)
		{
			return RemoveRenderer(instanceID, RendererUpdateStatus.RemovedAsUnsupported);
		}
		if (rendererDesc.MeshRenderer != null)
		{
			rendererDesc.MeshRenderer.SetPropertyBlock(null);
		}
		if (m_InstanceIDToRendererState.TryGetValue(instanceID, out var item))
		{
			UpdateRendererCustomPerInstanceData(in rendererDesc, in rendererParams, in changeContext, ref item);
			item.IsEnabled = rendererDesc.GetEnabled(in rendererParams);
			m_InstanceIDToRendererState[instanceID] = item;
		}
		return RendererUpdateStatus.Added;
	}

	private bool AreMaterialsSupported(List<Material> materialList)
	{
		foreach (Material material in materialList)
		{
			if (!ShaderMetadataRepository.IsMaterialSupported(material))
			{
				return false;
			}
		}
		return true;
	}

	private void TryRegisterLightmappingMaterial(in GPUDrivenRendererGroupPool.RendererGroupKey groupKey)
	{
		if ((groupKey.RendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightMaps) != 0)
		{
			ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(in groupKey);
			ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref ResourceRegistry.GetUnmanagedMaterialInfo(in groupKey);
			Lightmapping.TryRegisterLightmappedMaterial(managedMaterialInfo.OriginalMaterial, unmanagedMaterialInfo.OriginalBatchMaterialID, groupKey.RendererSettings.Scene);
		}
	}

	private Material CorrectMaterial([CanBeNull] Material material)
	{
		if (!(material == null))
		{
			return material;
		}
		return m_ErrorMaterial;
	}

	public RendererUpdateStatus RemoveRenderer(GPUDrivenInstanceID instanceID, RendererUpdateStatus removalStatus = RendererUpdateStatus.Removed)
	{
		if (!m_InstanceIDToRendererState.TryGetValue(instanceID, out var item))
		{
			return RendererUpdateStatus.CouldNotRegister;
		}
		for (int i = 0; i < item.SubmeshCount; i++)
		{
			InstanceKey instanceKey = default(InstanceKey);
			instanceKey.InstanceID = instanceID;
			instanceKey.SubmeshIndex = i;
			InstanceKey instanceKey2 = instanceKey;
			DeallocateInstance(instanceKey2);
		}
		m_InstanceIDToRendererState.Remove(instanceID);
		if (instanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject)
		{
			m_ComponentMapping.RemoveAllMappings(instanceID);
			m_LastFrameMovingRendererIDs.Remove(instanceID.RawInstanceID);
			m_MovingRendererIDs.Remove(instanceID.RawInstanceID);
		}
		return removalStatus;
	}

	private void UpdateRendererData(in RendererDesc rendererDesc, in RendererParams rendererParams, in ChangeContext changeContext, ref RendererState rendererState, ref GPUDrivenRendererGroupPool.RendererUpdateFlags updateFlags)
	{
		if (changeContext.FrameIndex <= rendererState.LastRendererDataChangeFrameIndex)
		{
			return;
		}
		rendererState.LastRendererDataChangeFrameIndex = changeContext.FrameIndex;
		for (int i = 0; i < rendererState.SubmeshCount; i++)
		{
			InstanceKey instanceKey = default(InstanceKey);
			instanceKey.InstanceID = rendererDesc.InstanceID;
			instanceKey.SubmeshIndex = i;
			InstanceKey key = instanceKey;
			if (!m_InstanceKeyToInstanceData.TryGetValue(key, out var item))
			{
				continue;
			}
			ModifyVisibilityInfo(item.IndexAllocation).VisibilityFlags = GetVisibilityFlags(in rendererDesc, in rendererParams);
			ref InstanceMetadata reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_InstanceMetadata, item.IndexAllocation.Index);
			reference.RendererInstanceID = rendererDesc.InstanceID;
			reference.SubmeshIndex = i;
			reference.GameObjectInstanceID = rendererDesc.GetGameObjectInstanceID(in rendererParams);
			reference.SceneIndex = GetSceneIndex(in rendererDesc, in rendererParams);
			ref GPUDrivenRendererGroupPool.RendererGroup reference2 = ref RendererGroupPool.Get(item.RendererGroupIndex);
			GPUDrivenRendererGroupPool.RendererSettings rendererSettings = GPUDrivenRendererGroupPool.RendererSettings.From(in rendererDesc, in rendererParams, m_RendererSettingsCreationInfo);
			GPUDrivenRendererGroupPool.RendererSettings rendererSettings2 = reference2.Key.RendererSettings;
			if (!rendererSettings2.Equals(rendererSettings))
			{
				GPUDrivenRendererGroupPool.RendererGroupKey groupKey = reference2.Key;
				groupKey.RendererSettings = rendererSettings;
				TryRegisterLightmappingMaterial(in groupKey);
				if (RendererGroupPool.TryMigrateInstanceSimple(ref item, in groupKey))
				{
					updateFlags |= GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData;
					m_InstanceKeyToInstanceData[key] = item;
				}
				int num = ComputeTotalInstanceDataSize(in ResourceRegistry.GetManagedMaterialInfo(groupKey.MaterialAllocation), in ResourceRegistry.GetUnmanagedMaterialInfo(groupKey.MaterialAllocation), in groupKey.RendererSettings);
				if (num != item.InstanceSize)
				{
					PersistentData.FreeInstanceData(item.IndexAllocation);
					PersistentData.AllocateInstanceData(item.IndexAllocation, num);
					updateFlags |= GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData;
				}
			}
			int sortingOrder = rendererDesc.GetSortingOrder(in rendererParams);
			if (sortingOrder != reference.SortingOrder)
			{
				reference.SortingOrder = sortingOrder;
				ref GPUDrivenRendererGroupPool.RendererGroup rendererGroup = ref RendererGroupPool.Get(item.RendererGroupIndex);
				RendererGroupPool.ScheduleSorting(ref rendererGroup);
			}
		}
		if (rendererDesc.InstanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject && m_LastFrameMovingRendererIDs.Contains(rendererDesc.InstanceID.RawInstanceID) && !rendererDesc.GetEnabled(in rendererParams))
		{
			m_LastFrameMovingRendererIDs.Remove(rendererDesc.InstanceID.RawInstanceID);
		}
	}

	private GPUDrivenIndexAllocator.IndexAllocation GetSceneIndex(in RendererDesc rendererDesc, in RendererParams rendererParams)
	{
		Scene scene = rendererDesc.GetScene(in rendererParams);
		GPUDrivenSceneHandle sceneHandle = GPUDrivenSceneHandle.FromScene(in scene);
		return GetSceneIndex(sceneHandle);
	}

	private GPUDrivenIndexAllocator.IndexAllocation GetSceneIndex(GPUDrivenSceneHandle sceneHandle)
	{
		if (!m_PersistentSceneData.TryGetSceneIndex(sceneHandle, out var sceneIndex))
		{
			return GPUDrivenIndexAllocator.IndexAllocation.Invalid;
		}
		return sceneIndex;
	}

	[MustUseReturnValue]
	private bool UpdateMaterialData(in RendererDesc rendererDesc, in RendererParams rendererParams, in ChangeContext changeContext, ref RendererState rendererState, ref GPUDrivenRendererGroupPool.RendererUpdateFlags updateFlags)
	{
		if (changeContext.FrameIndex <= rendererState.LastMaterialDataChangeFrameIndex)
		{
			return true;
		}
		rendererState.LastMaterialDataChangeFrameIndex = changeContext.FrameIndex;
		List<Material> tempMaterialList = m_TempMaterialList;
		rendererDesc.GetMaterials(in rendererParams, tempMaterialList);
		Mesh mesh = null;
		bool flag = false;
		int i;
		GPUDrivenRendererGroupPool.RendererGroupKey groupKey = default(GPUDrivenRendererGroupPool.RendererGroupKey);
		for (i = 0; i < rendererState.SubmeshCount; i++)
		{
			InstanceKey instanceKey = default(InstanceKey);
			instanceKey.InstanceID = rendererDesc.InstanceID;
			instanceKey.SubmeshIndex = i;
			InstanceKey instanceKey2 = instanceKey;
			if (!m_InstanceKeyToInstanceData.TryGetValue(instanceKey2, out var item))
			{
				continue;
			}
			if (i < tempMaterialList.Count)
			{
				Material material = CorrectMaterial(tempMaterialList[i]);
				int rendererGroupIndex = item.RendererGroupIndex;
				GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey = RendererGroupPool.Get(rendererGroupIndex).Key;
				Material originalMaterial = ResourceRegistry.GetManagedMaterialInfo(in rendererGroupKey).OriginalMaterial;
				mesh = ResourceRegistry.GetManagedMeshInfo(in rendererGroupKey).Mesh;
				if (!(material != originalMaterial))
				{
					continue;
				}
				groupKey.MeshAllocation = ResourceRegistry.GetOrRegisterMesh(mesh, i);
				groupKey.MaterialAllocation = ResourceRegistry.GetOrRegisterMaterial(material);
				if (groupKey.MaterialAllocation.Equals(GPUDrivenIndexAllocator.IndexAllocation.Invalid))
				{
					GPUDrivenErrorLogging.FailedToAllocateMaterial(material);
					flag = true;
					continue;
				}
				groupKey.RendererSettings = GPUDrivenRendererGroupPool.RendererSettings.From(in rendererDesc, in rendererParams, m_RendererSettingsCreationInfo);
				TryRegisterLightmappingMaterial(in groupKey);
				int orAllocate = RendererGroupPool.GetOrAllocate(in groupKey);
				if (orAllocate != -1)
				{
					ref GPUDrivenRendererGroupPool.RendererGroup reference = ref RendererGroupPool.Get(orAllocate);
					RendererGroupPool.AddInstanceToGroup(ref reference, item.IndexAllocation);
					item.RendererGroupIndex = orAllocate;
					ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(in reference);
					ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref ResourceRegistry.GetUnmanagedMaterialInfo(in reference);
					GPUDrivenAllocator.DataAllocation instanceAllocation = GetInstanceDataAllocation(in item);
					int num = ComputeTotalInstanceDataSize(in managedMaterialInfo, in unmanagedMaterialInfo, in reference.Key.RendererSettings);
					if (num != item.InstanceSize)
					{
						PersistentData.FreeInstanceData(item.IndexAllocation);
						GPUDrivenAllocator.DataAllocation dataAllocation = PersistentData.AllocateInstanceData(item.IndexAllocation, num);
						item.InstanceSize = num;
						instanceAllocation = dataAllocation;
						updateFlags |= GPUDrivenRendererGroupPool.RendererUpdateFlags.RendererData;
					}
					PersistentData.BeginWritingPerInstanceMetadata().Write(item.IndexAllocation.Index, instanceAllocation, unmanagedMaterialInfo.MaterialDataAllocation);
					m_InstanceKeyToInstanceData[instanceKey2] = item;
				}
				RendererGroupPool.RemoveInstanceFromGroup(ref RendererGroupPool.Get(rendererGroupIndex), item.IndexAllocation);
				updateFlags |= GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData;
			}
			else
			{
				DeallocateInstance(instanceKey2);
			}
		}
		if (!flag)
		{
			if (tempMaterialList.Count > rendererState.SubmeshCount && mesh != null)
			{
				GPUDrivenRendererGroupPool.RendererGroupKey groupKey2 = default(GPUDrivenRendererGroupPool.RendererGroupKey);
				for (; i < tempMaterialList.Count; i++)
				{
					Material material2 = CorrectMaterial(tempMaterialList[i]);
					groupKey2.MeshAllocation = ResourceRegistry.GetOrRegisterMesh(mesh, i);
					groupKey2.MaterialAllocation = ResourceRegistry.GetOrRegisterMaterial(material2);
					if (groupKey2.MaterialAllocation.Equals(GPUDrivenIndexAllocator.IndexAllocation.Invalid))
					{
						GPUDrivenErrorLogging.FailedToAllocateMaterial(material2);
						flag = true;
						continue;
					}
					groupKey2.RendererSettings = GPUDrivenRendererGroupPool.RendererSettings.From(in rendererDesc, in rendererParams, m_RendererSettingsCreationInfo);
					TryRegisterLightmappingMaterial(in groupKey2);
					int orAllocate2 = RendererGroupPool.GetOrAllocate(in groupKey2);
					if (orAllocate2 == -1)
					{
						flag = true;
						continue;
					}
					InstanceKey instanceKey = default(InstanceKey);
					instanceKey.InstanceID = rendererDesc.InstanceID;
					instanceKey.SubmeshIndex = i;
					InstanceKey instanceKey3 = instanceKey;
					TryAllocateInstance(in rendererDesc, in rendererParams, instanceKey3, orAllocate2, in changeContext, out var _);
				}
				updateFlags |= GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData;
			}
			rendererState.SubmeshCount = tempMaterialList.Count;
		}
		tempMaterialList.Clear();
		return !flag;
	}

	private ref GPUDrivenAllocator.DataAllocation GetInstanceDataAllocation(in InstanceData instanceData)
	{
		return ref GetInstanceDataAllocation(in instanceData.IndexAllocation);
	}

	private ref GPUDrivenAllocator.DataAllocation GetInstanceDataAllocation(in GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref PersistentData.GetInstanceDataAllocation(indexAllocation.Index);
	}

	private void UpdateRendererCustomPerInstanceData(in RendererDesc rendererDesc, in RendererParams rendererParams, in ChangeContext changeContext, ref RendererState rendererState)
	{
		if (changeContext.FrameIndex <= rendererState.LastCustomPerInstanceDataChangeFrameIndex)
		{
			return;
		}
		GPUDrivenInstanceID instanceID = rendererDesc.InstanceID;
		rendererState.LastCustomPerInstanceDataChangeFrameIndex = changeContext.FrameIndex;
		GPUDrivenRenderer gpuDrivenRenderer = null;
		if (instanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject)
		{
			if (TryGetActiveGPUDrivenRenderer(in rendererDesc, out gpuDrivenRenderer))
			{
				GPUDrivenInstanceID gpuDrivenRendererInstanceID = GPUDrivenInstanceID.UnityObject(gpuDrivenRenderer.GetInstanceID());
				m_ComponentMapping.AddGPUDrivenRendererMapping(instanceID, gpuDrivenRendererInstanceID);
			}
			else
			{
				gpuDrivenRenderer = null;
				m_ComponentMapping.RemoveGPUDrivenRendererMapping(instanceID);
			}
		}
		NativeList<GPUDrivenRenderer.PropertyData> result = new NativeList<GPUDrivenRenderer.PropertyData>(Allocator.Temp);
		for (int i = 0; i < rendererState.SubmeshCount; i++)
		{
			InstanceKey instanceKey = default(InstanceKey);
			instanceKey.InstanceID = instanceID;
			instanceKey.SubmeshIndex = i;
			InstanceKey key = instanceKey;
			if (!m_InstanceKeyToInstanceData.TryGetValue(key, out var item))
			{
				continue;
			}
			ref GPUDrivenRendererGroupPool.RendererGroup reference = ref RendererGroupPool.Get(item.RendererGroupIndex);
			ref GPUDrivenAllocator.DataAllocation instanceDataAllocation = ref GetInstanceDataAllocation(in item);
			ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(in reference);
			result.Clear();
			CollectPerInstanceData(gpuDrivenRenderer, in rendererParams, in managedMaterialInfo, result);
			GPUDrivenPropertyDataWriter writer = PersistentData.BeginWritingProperties();
			writer.SkipPropertyDataRaw<GPUDrivenMetadataAuthoring.DefaultPerInstanceData>();
			writer.SkipAllOptionalPerInstanceData(in reference.Key);
			foreach (GPUDrivenRenderer.PropertyData item2 in result)
			{
				GPUDrivenRenderer.PropertyData propertyData = item2;
				writer.WritePropertyData(in instanceDataAllocation, in propertyData);
			}
		}
	}

	private static bool TryGetActiveGPUDrivenRenderer(in RendererDesc rendererDesc, out GPUDrivenRenderer gpuDrivenRenderer)
	{
		if (rendererDesc.MeshRenderer != null && rendererDesc.MeshRenderer.TryGetComponent<GPUDrivenRenderer>(out var component) && component.enabled)
		{
			gpuDrivenRenderer = component;
			return true;
		}
		gpuDrivenRenderer = null;
		return false;
	}

	private static GPUDrivenVisibilityFlags GetVisibilityFlags(in RendererDesc rendererDesc, in RendererParams rendererParams)
	{
		return rendererDesc.ExtraVisibilityFlags;
	}

	public bool TryMapGPUDrivenRendererInstanceIDToMeshRenderer(GPUDrivenInstanceID gpuDrivenRendererInstanceID, out GPUDrivenInstanceID rendererInstanceID)
	{
		return m_ComponentMapping.TryMapGPUDrivenRendererInstanceIDToMeshRenderer(gpuDrivenRendererInstanceID, out rendererInstanceID);
	}

	public bool TryMapMeshFilterInstanceIDToMeshRenderer(GPUDrivenInstanceID meshFilterInstanceID, out GPUDrivenInstanceID rendererInstanceID)
	{
		return m_ComponentMapping.TryMapMeshFilterInstanceIDToMeshRenderer(meshFilterInstanceID, out rendererInstanceID);
	}

	private bool TryAllocateInstance(in RendererDesc rendererDesc, in RendererParams rendererParams, InstanceKey instanceKey, int rendererGroupIndex, in ChangeContext changeContext, out int instanceSize)
	{
		GPUDrivenIndexAllocator.IndexAllocation indexAllocation = AllocateInstanceIndexAndGrowIfNeeded();
		if (indexAllocation.Index == -1)
		{
			GPUDrivenErrorLogging.FailedToAllocateInstanceIndex(in rendererDesc, instanceKey.SubmeshIndex);
			instanceSize = 0;
			return false;
		}
		m_InstancesCreatedThisFrame.Add(in indexAllocation.Index);
		InstanceData instanceData = default(InstanceData);
		instanceData.IndexAllocation = indexAllocation;
		instanceData.RendererGroupIndex = rendererGroupIndex;
		InstanceData item = instanceData;
		if (!m_InstanceIDToRendererState.TryGetValue(instanceKey.InstanceID, out var item2))
		{
			item2 = RendererState.Empty;
		}
		item2.SubmeshCount++;
		item2.LastRendererDataChangeFrameIndex = changeContext.FrameIndex;
		item2.LastMeshChangeFrameIndex = changeContext.FrameIndex;
		item2.LastMaterialDataChangeFrameIndex = changeContext.FrameIndex;
		ref GPUDrivenRendererGroupPool.RendererGroup reference = ref RendererGroupPool.Get(rendererGroupIndex);
		ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(in reference);
		ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref ResourceRegistry.GetUnmanagedMaterialInfo(in reference);
		item.InstanceSize = ComputeTotalInstanceDataSize(in managedMaterialInfo, in unmanagedMaterialInfo, in reference.Key.RendererSettings);
		GPUDrivenAllocator.DataAllocation dataAllocation = PersistentData.AllocateInstanceData(indexAllocation, item.InstanceSize);
		if (!dataAllocation.IsValid())
		{
			GPUDrivenErrorLogging.FailedToAllocatePersistentData(in rendererDesc, instanceKey.SubmeshIndex);
			instanceSize = 0;
			return false;
		}
		PersistentData.BeginWritingPerInstanceMetadata().Write(indexAllocation.Index, dataAllocation, unmanagedMaterialInfo.MaterialDataAllocation);
		PersistentData.BeginWritingProperties(autoMarkDirty: false).MarkWholeAllocationDirty(in dataAllocation);
		m_InstanceIDToRendererState[instanceKey.InstanceID] = item2;
		m_InstanceKeyToInstanceData.Add(instanceKey, item);
		ModifyVisibilityInfo(item.IndexAllocation) = new GPUDrivenVisibilityInfo
		{
			VisibilityFlags = GetVisibilityFlags(in rendererDesc, in rendererParams)
		};
		m_InstanceMetadata[item.IndexAllocation.Index] = new InstanceMetadata
		{
			RendererInstanceID = instanceKey.InstanceID,
			SubmeshIndex = instanceKey.SubmeshIndex,
			GameObjectInstanceID = rendererDesc.GetGameObjectInstanceID(in rendererParams),
			SerialNumber = m_InstanceSerialNumber,
			SortingOrder = rendererDesc.GetSortingOrder(in rendererParams),
			SceneIndex = GetSceneIndex(in rendererDesc, in rendererParams)
		};
		m_InstanceSerialNumber++;
		RendererGroupPool.AddInstanceToGroup(ref reference, indexAllocation);
		m_TetrahedronCacheIndices[indexAllocation.Index] = -1;
		instanceSize = item.InstanceSize;
		return true;
	}

	private GPUDrivenIndexAllocator.IndexAllocation AllocateInstanceIndexAndGrowIfNeeded()
	{
		GPUDrivenIndexAllocator.IndexAllocation result = m_InstanceIndexAllocator.Allocate();
		if (result.Index != -1)
		{
			return result;
		}
		GrowInstanceCount();
		return m_InstanceIndexAllocator.Allocate();
	}

	private ref GPUDrivenVisibilityInfo ModifyVisibilityInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref PersistentData.ModifyVisibilityInfo(indexAllocation);
	}

	private static void CollectPerInstanceData([CanBeNull] GPUDrivenRenderer gpuDrivenRenderer, in RendererParams rendererParams, in GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo, NativeList<GPUDrivenRenderer.PropertyData> result)
	{
		for (int i = 0; i < managedMaterialInfo.PropertyLayout.PerInstanceData.Length; i++)
		{
			ref GPUDrivenRenderer.PropertyData reference = ref UnsafeCollectionExtensions.ElementAsRef(in managedMaterialInfo.PropertyLayout.PerInstanceData, i);
			if (!rendererParams.TryGetPerInstanceData(gpuDrivenRenderer, reference.NameID, out var propertyData))
			{
				propertyData = reference;
			}
			result.Add(in propertyData);
		}
	}

	private static int ComputeTotalInstanceDataSize(in GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo, in GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo, in GPUDrivenRendererGroupPool.RendererSettings rendererSettings)
	{
		GPUDrivenMetadataAuthoring.MetadataComponents metadataComponents = rendererSettings.GetMetadataComponents();
		return unmanagedMaterialInfo.BatchCollection.Get(in metadataComponents).BuiltInPerInstanceDataSize + managedMaterialInfo.PropertyLayout.PerInstanceDataSizeInBytes;
	}

	private void DeallocateInstance(InstanceKey instanceKey)
	{
		if (m_InstanceKeyToInstanceData.TryGetValue(instanceKey, out var item))
		{
			PersistentData.FreeInstanceData(item.IndexAllocation);
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = item.IndexAllocation;
			m_InstanceIndexAllocator.Free(indexAllocation);
			ref GPUDrivenRendererGroupPool.RendererGroup rendererGroup = ref RendererGroupPool.Get(item.RendererGroupIndex);
			RendererGroupPool.RemoveInstanceFromGroup(ref rendererGroup, indexAllocation);
			m_TetrahedronCacheIndices[indexAllocation.Index] = -1;
		}
		m_InstanceKeyToInstanceData.Remove(instanceKey);
	}

	public RendererUpdateStatus UpdateRendererMesh(in RendererDesc rendererDesc, in ChangeContext changeContext, in RendererParams rendererParams = default(RendererParams), bool forceUpdate = false)
	{
		if (!rendererDesc.TryGetMesh(in rendererParams, out var meshFilter, out var mesh))
		{
			mesh = null;
		}
		GPUDrivenInstanceID instanceID = rendererDesc.InstanceID;
		if (mesh == null || !GPUDrivenRenderingUtils.IsRendererSupported(in rendererDesc))
		{
			return RemoveRenderer(instanceID);
		}
		if (!m_InstanceIDToRendererState.TryGetValue(instanceID, out var item))
		{
			return AddRenderer(in rendererDesc, in rendererParams, in changeContext);
		}
		if (changeContext.FrameIndex <= item.LastMeshChangeFrameIndex)
		{
			return RendererUpdateStatus.DidNotUpdateRegistered;
		}
		item.LastMeshChangeFrameIndex = changeContext.FrameIndex;
		m_InstanceIDToRendererState[instanceID] = item;
		bool flag = false;
		if (forceUpdate)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < item.SubmeshCount; i++)
			{
				InstanceKey instanceKey = default(InstanceKey);
				instanceKey.InstanceID = instanceID;
				instanceKey.SubmeshIndex = i;
				InstanceKey key = instanceKey;
				if (m_InstanceKeyToInstanceData.TryGetValue(key, out var item2))
				{
					ref GPUDrivenRendererGroupPool.RendererGroup rendererGroup = ref RendererGroupPool.Get(item2.RendererGroupIndex);
					if (ResourceRegistry.GetManagedMeshInfo(in rendererGroup).Mesh != mesh)
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			return RendererUpdateStatus.DidNotUpdateRegistered;
		}
		if (meshFilter != null)
		{
			m_ComponentMapping.AddMeshFilterMapping(instanceID, GPUDrivenInstanceID.UnityObject(meshFilter.GetInstanceID()));
		}
		GPUDrivenRendererGroupPool.RendererGroupKey groupKey = default(GPUDrivenRendererGroupPool.RendererGroupKey);
		for (int j = 0; j < item.SubmeshCount; j++)
		{
			InstanceKey instanceKey = default(InstanceKey);
			instanceKey.InstanceID = instanceID;
			instanceKey.SubmeshIndex = j;
			InstanceKey key2 = instanceKey;
			if (m_InstanceKeyToInstanceData.TryGetValue(key2, out var item3))
			{
				ref GPUDrivenRendererGroupPool.RendererGroup rendererGroup2 = ref RendererGroupPool.Get(item3.RendererGroupIndex);
				ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref ResourceRegistry.GetManagedMaterialInfo(in rendererGroup2);
				groupKey.MeshAllocation = ResourceRegistry.GetOrRegisterMesh(mesh, j);
				groupKey.MaterialAllocation = ResourceRegistry.GetOrRegisterMaterial(managedMaterialInfo.OriginalMaterial).EnsureAllocationValidity();
				groupKey.RendererSettings = GPUDrivenRendererGroupPool.RendererSettings.From(in rendererDesc, in rendererParams, m_RendererSettingsCreationInfo);
				TryRegisterLightmappingMaterial(in groupKey);
				if (RendererGroupPool.TryMigrateInstanceSimple(ref item3, in groupKey))
				{
					m_InstanceKeyToInstanceData[key2] = item3;
				}
			}
		}
		return RendererUpdateStatus.UpdatedRegistered;
	}

	private void UpdateAmbientProbe()
	{
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		if (!(m_CachedAmbientProbe != ambientProbe))
		{
			return;
		}
		NativeList<int> nativeList = new NativeList<int>(m_InstanceIDToRendererState.Count, Allocator.Temp);
		foreach (KVPair<GPUDrivenInstanceID, RendererState> item in m_InstanceIDToRendererState)
		{
			GPUDrivenInstanceID key = item.Key;
			if (key.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject)
			{
				MeshRenderer meshRenderer = ObjectDispatcherService.FindByInstanceId<MeshRenderer>(key.RawInstanceID);
				if (meshRenderer != null && meshRenderer.enabled && meshRenderer.gameObject.activeInHierarchy)
				{
					nativeList.Add(in key.RawInstanceID);
				}
			}
		}
		NativeArray<int> changedRendererIDs = nativeList.AsArray();
		ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
		EnableGPUDrivenRenderingAndLoadNativeData(changedRendererIDs, in changeContext);
		m_CachedAmbientProbe = ambientProbe;
	}

	public void EnableGPUDrivenRenderingAndLoadNativeData(NativeArray<int> changedRendererIDs, in ChangeContext changeContext, NativeList<int> invalidRendererIDs = default(NativeList<int>))
	{
		if (changedRendererIDs.Length != 0)
		{
			NativeDataUpdateValidator.ValidateUnregisteredRenderers(this, changedRendererIDs);
			GPUDrivenProcessor.NativeRenderersData nativeRenderersData = m_GPUDrivenProcessor.EnableGPUDrivenRenderingAndFetchNativeRenderersData(changedRendererIDs, Allocator.TempJob);
			UpdateNativeData(nativeRenderersData, in changeContext, GPUDrivenInstanceID.InstanceIDType.UnityObject);
			if (nativeRenderersData.InvalidRendererIDs.Length > 0 && invalidRendererIDs.IsCreated)
			{
				invalidRendererIDs.AddRange(nativeRenderersData.InvalidRendererIDs);
				NativeDataUpdateValidator.ValidateInvalidRenderers(nativeRenderersData);
			}
			nativeRenderersData.Dispose();
		}
	}

	public void UpdateNativeData(GPUDrivenProcessor.NativeRenderersData nativeRenderersData, in ChangeContext changeContext, GPUDrivenInstanceID.InstanceIDType objectType)
	{
		using (new ProfilingScope(Profiling.UpdateNativeData))
		{
			GPUDrivenPropertyDataWriter propertyDataWriter = PersistentData.BeginWritingProperties(autoMarkDirty: false);
			int num = nativeRenderersData.RendererIDs.Length * 64;
			NativeList<GPUDrivenNativeDataUpdate.Instance> nativeList = new NativeList<GPUDrivenNativeDataUpdate.Instance>(num, Allocator.TempJob);
			NativeList<int> nativeList2 = new NativeList<int>(num, Allocator.TempJob);
			CollectInstancesForNativeDataUpdateJob jobData = default(CollectInstancesForNativeDataUpdateJob);
			jobData.PropertyDataWriter = propertyDataWriter;
			jobData.ChangeContext = changeContext;
			jobData.ObjectType = objectType;
			jobData.UpdatedInstances = nativeList.AsParallelWriter();
			jobData.RendererIDs = nativeRenderersData.RendererIDs.AsReadOnly();
			jobData.InstanceKeyToInstanceData = m_InstanceKeyToInstanceData.AsReadOnly();
			jobData.InstanceIDToRendererState = m_InstanceIDToRendererState;
			jobData.RendererGroups = RendererGroupPool.GetInnerPool();
			jobData.InstanceDataAllocations = PersistentData.GetInstanceDataAllocationsReadonly();
			JobHandle dependsOn = IJobParallelForBatchExtensions.Schedule(jobData, nativeRenderersData.RendererIDs.Length, 32);
			LightProbesQuery lightProbesQuery = new LightProbesQuery(Allocator.TempJob);
			GPUDrivenNativeDataUpdate.Job jobData2 = default(GPUDrivenNativeDataUpdate.Job);
			jobData2.LightProbesQuery = lightProbesQuery;
			jobData2.UpdatedInstances = nativeList.AsDeferredJobArray();
			jobData2.RendererGroups = RendererGroupPool.GetInnerPool();
			jobData2.MeshInfos = ResourceRegistry.GetInnerUnmanagedMeshPool();
			jobData2.MaterialInfos = ResourceRegistry.GetInnerUnmanagedMaterialPool();
			jobData2.InstanceMetadata = m_InstanceMetadata.AsReadOnly();
			jobData2.SceneData = m_PersistentSceneData.GetSceneDataReadonly();
			jobData2.LocalToWorldMatrices = nativeRenderersData.LocalToWorldMatrix;
			jobData2.PrevLocalToWorldMatrices = nativeRenderersData.PrevLocalToWorldMatrix;
			jobData2.LocalBounds = nativeRenderersData.LocalBounds;
			jobData2.LightmapSTs = nativeRenderersData.LightmapSTs;
			jobData2.LightmapIndices = nativeRenderersData.LightmapIndices;
			jobData2.LODGroupIDs = nativeRenderersData.LODGroupIDs;
			jobData2.PackedRendererData = nativeRenderersData.PackedRendererData;
			jobData2.VisibilityInfo = PersistentData.GetAllVisibilityInfoRaw();
			jobData2.TetrahedronCacheIndices = m_TetrahedronCacheIndices;
			jobData2.LODGroupMetadata = LODGroupRepository.GetGroupMetadataReadonly();
			jobData2.MovingRendererIDs = nativeList2.AsParallelWriter();
			JobHandle jobHandle = IJobParallelForExtensions.Schedule(jobData2, num, 8, dependsOn);
			lightProbesQuery.Dispose(jobHandle);
			GPUDrivenNativeDataUpdate.UpdateMovingRendererIDsJob jobData3 = default(GPUDrivenNativeDataUpdate.UpdateMovingRendererIDsJob);
			jobData3.CurrentMovingRendererIDs = nativeList2.AsDeferredJobArray();
			jobData3.MovingRendererIDs = m_MovingRendererIDs;
			jobData3.LastFrameMovingRendererIDs = m_LastFrameMovingRendererIDs;
			jobHandle = jobData3.Schedule(jobHandle);
			JobHandle.CombineDependencies(jobHandle, new GPUDrivenNativeDataUpdate.MarkInstancesDirtyJob
			{
				Instances = nativeList.AsDeferredJobArray(),
				InstanceDataAllocations = PersistentData.GetInstanceDataAllocationsReadonly(),
				PropertyDataWriter = propertyDataWriter,
				DirtyVisibilityInfoSegmentList = PersistentData.GetDirtyVisibilityInfoSegmentList()
			}.Schedule(dependsOn)).Complete();
			nativeList.Dispose();
			nativeList2.Dispose();
		}
	}

	public void DisableGPURendering(NativeArray<int> rendererIDs)
	{
		m_GPUDrivenProcessor.DisableGPUDrivenRendering(rendererIDs);
	}

	public NativeArray<GPUDrivenCullingContext> GetCullingContextsAndClear(Allocator allocator)
	{
		return InstanceCuller.GetCullingContextsAndClear(allocator);
	}

	private JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext batchCullingContext, BatchCullingOutput cullingOutput, IntPtr userContext)
	{
		using (new ProfilingScope(Profiling.OnPerformCulling))
		{
			if (!m_Initialized)
			{
				return default(JobHandle);
			}
			if (ScriptableRenderer.Current is WaaaghRenderer waaaghRenderer && !waaaghRenderer.Settings.DrawViaGPUDrivenBRG)
			{
				return default(JobHandle);
			}
			return batchCullingContext.viewType switch
			{
				BatchCullingViewType.Unknown => default(JobHandle), 
				BatchCullingViewType.Camera => InstanceCuller.OnPerformMainPassCulling(in batchCullingContext, cullingOutput), 
				BatchCullingViewType.Light => InstanceCuller.OnPerformMainPassCulling(in batchCullingContext, cullingOutput), 
				BatchCullingViewType.Picking => InstanceCuller.OnPerformPickingCulling(in batchCullingContext, cullingOutput), 
				BatchCullingViewType.SelectionOutline => InstanceCuller.OnPerformPickingCulling(in batchCullingContext, cullingOutput), 
				BatchCullingViewType.Filtering => InstanceCuller.OnPerformFilteringCulling(in batchCullingContext, cullingOutput), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	public void PreRender(CommandBuffer cmd)
	{
		using (new ProfilingScope(Profiling.PreRender))
		{
			if (m_LastFrameMovingRendererIDs.Count > 0)
			{
				using (new ProfilingScope(Profiling.LastFrameTransformUpdates))
				{
					NativeArray<int> changedRendererIDs = m_LastFrameMovingRendererIDs.ToNativeArray(Allocator.Temp);
					ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
					EnableGPUDrivenRenderingAndLoadNativeData(changedRendererIDs, in changeContext);
					m_LastFrameMovingRendererIDs.Clear();
				}
			}
			GPUDrivenResourceRegistry.Updates resourceUpdatesAndClear = ResourceRegistry.GetResourceUpdatesAndClear(Allocator.Temp);
			if (resourceUpdatesAndClear.CreatedMaterialIDs.Length > 0)
			{
				this.OnCreatedMaterials?.Invoke(resourceUpdatesAndClear.CreatedMaterialIDs.AsArray().AsReadOnly());
			}
			GPUDrivenBRGDebug debugData = DebugData;
			if (debugData != null && debugData.ForceBuildRenderGroupsEachFrame)
			{
				RendererGroupPool.OnModifiedCPUData();
			}
			using (new ProfilingScope(cmd, Profiling.PreRender))
			{
				GPUDrivenPersistentData.PendingGPUUpload pendingGPUUpload = PersistentData.BeginGPUUpload();
				RendererGroupPool.BeginRebuildingGroupInfo();
				LODGroupRepository.PreRender();
				InstanceCuller.PreRender();
				PersistentData.CompleteGPUUpload(cmd, ref pendingGPUUpload);
			}
		}
	}

	public void PostRender()
	{
		using (new ProfilingScope(Profiling.PostRender))
		{
			RendererGroupPool.EarlyPostRender();
			InstanceCuller.EarlyPostRender();
			RendererGroupPool.PostRender();
			InstanceCuller.PostRender();
			m_BatchedDataUploader.PostRender();
			m_InstancesCreatedThisFrame.Clear();
			m_InstanceSerialNumber++;
			NativeHashSet<int> movingRendererIDs = m_MovingRendererIDs;
			NativeHashSet<int> lastFrameMovingRendererIDs = m_LastFrameMovingRendererIDs;
			m_LastFrameMovingRendererIDs = movingRendererIDs;
			m_MovingRendererIDs = lastFrameMovingRendererIDs;
			m_MovingRendererIDs.Clear();
		}
	}

	internal void BatchProcessMeshDestruction(in ChangeContext changeContext)
	{
		NativeList<int> nativeList = new NativeList<int>(Allocator.Temp);
		foreach (KVPair<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> activeRendererGroup in ActiveRendererGroups)
		{
			GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey = activeRendererGroup.Key;
			if (ResourceRegistry.GetManagedMeshInfo(in rendererGroupKey).Mesh == null)
			{
				int index = activeRendererGroup.Value.Index;
				ref UnsafeList<int> indices = ref RendererGroupPool.GetIndices(index);
				nativeList.AddRange(indices);
			}
		}
		if (nativeList.Length == 0)
		{
			return;
		}
		foreach (int item in nativeList)
		{
			GameObject gameObject = ObjectDispatcherService.FindByInstanceId<GameObject>(m_InstanceMetadata[item].GameObjectInstanceID);
			if (gameObject != null && gameObject.TryGetComponent<MeshRenderer>(out var component) && gameObject.TryGetComponent<MeshFilter>(out var component2))
			{
				RendererDesc rendererDesc = RendererDesc.FromMeshRenderer(component, component2);
				RendererParams rendererParams = default(RendererParams);
				UpdateRendererMesh(in rendererDesc, in changeContext, in rendererParams);
			}
		}
	}

	internal void BatchUpdateMeshAssets(List<UnityEngine.Object> changedMeshes, in ChangeContext changeContext)
	{
		NativeList<GPUDrivenIndexAllocator.IndexAllocation> nativeList = new NativeList<GPUDrivenIndexAllocator.IndexAllocation>(ResourceRegistry.MeshPool.Count, Allocator.Temp);
		foreach (UnityEngine.Object changedMesh in changedMeshes)
		{
			ResourceRegistry.UpdateMeshAsset((Mesh)changedMesh, nativeList);
		}
		if (nativeList.Length == 0)
		{
			return;
		}
		NativeList<int> nativeList2 = new NativeList<int>(Allocator.Temp);
		foreach (KVPair<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> activeRendererGroup in ActiveRendererGroups)
		{
			GPUDrivenIndexAllocator.IndexAllocation meshAllocation = activeRendererGroup.Key.MeshAllocation;
			if (nativeList.Contains(meshAllocation))
			{
				int index = activeRendererGroup.Value.Index;
				ref UnsafeList<int> indices = ref RendererGroupPool.GetIndices(index);
				nativeList2.AddRange(indices);
			}
		}
		if (nativeList2.Length == 0)
		{
			return;
		}
		NativeList<int> nativeList3 = new NativeList<int>(Allocator.Temp);
		foreach (int item in nativeList2)
		{
			ref InstanceMetadata reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_InstanceMetadata, item);
			GameObject gameObject = ObjectDispatcherService.FindByInstanceId<GameObject>(reference.GameObjectInstanceID);
			if (gameObject != null && gameObject.TryGetComponent<MeshRenderer>(out var component) && gameObject.TryGetComponent<MeshFilter>(out var component2))
			{
				RendererDesc rendererDesc = RendererDesc.FromMeshRenderer(component, component2);
				RendererParams rendererParams = default(RendererParams);
				if (UpdateRendererMesh(in rendererDesc, in changeContext, in rendererParams, forceUpdate: true) == RendererUpdateStatus.UpdatedRegistered)
				{
					nativeList3.Add(in reference.RendererInstanceID.RawInstanceID);
				}
			}
		}
		if (nativeList3.Length > 0)
		{
			EnableGPUDrivenRenderingAndLoadNativeData(invalidRendererIDs: new NativeList<int>(Allocator.Temp), changedRendererIDs: nativeList3.AsArray(), changeContext: in changeContext);
		}
		if (nativeList2.Length > 0)
		{
			RendererGroupPool.OnModifiedCPUData();
		}
	}

	public void BatchProcessMaterialUpdate(List<UnityEngine.Object> changed, ReadOnlySpan<int> destroyedID, in ChangeContext changeContext)
	{
		foreach (Material item in changed)
		{
			int instanceID = item.GetInstanceID();
			if (ResourceRegistry.TryGetMaterialIndexAllocation(instanceID, out var indexAllocation))
			{
				ResourceRegistry.SubstituteMaterialBatch(indexAllocation, item);
			}
			Lightmapping.OnMaterialUpdated(item);
		}
		NativeList<GPUDrivenIndexAllocator.IndexAllocation> drasticallyChangedMaterials = new NativeList<GPUDrivenIndexAllocator.IndexAllocation>(changed.Count, Allocator.Temp);
		foreach (Material item2 in changed)
		{
			if (TryFindMaterialIndexAllocation(item2, out var indexAllocation2))
			{
				ProcessMaterialChanges(indexAllocation2, shaderMetadataChanged: false, drasticallyChangedMaterials);
			}
		}
		if (drasticallyChangedMaterials.Length > 0)
		{
			ProcessDrasticallyChangedMaterials(drasticallyChangedMaterials.AsArray(), in changeContext);
		}
		ReadOnlySpan<int> readOnlySpan = destroyedID;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			int materialInstanceID = readOnlySpan[i];
			if (ResourceRegistry.TryGetMaterialIndexAllocation(materialInstanceID, out var indexAllocation3))
			{
				ResourceRegistry.SubstituteMaterialBatch(indexAllocation3, m_ErrorMaterial);
			}
		}
	}
}
