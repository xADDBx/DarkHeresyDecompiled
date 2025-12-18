using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenResourceRegistry : IDisposable, IGPUDrivenMemoryProfilingSource
{
	[Flags]
	public enum MaterialFlags
	{
		None = 0,
		OpaqueDistortion = 4,
		Transparent = 8
	}

	internal struct Updates : IDisposable
	{
		public NativeList<int> CreatedMaterialIDs;

		public NativeList<int> ChangedMaterialIDs;

		public Updates(Allocator allocator)
		{
			CreatedMaterialIDs = new NativeList<int>(allocator);
			ChangedMaterialIDs = new NativeList<int>(allocator);
		}

		public void Dispose()
		{
			CreatedMaterialIDs.Dispose();
			ChangedMaterialIDs.Dispose();
		}

		public void Clear()
		{
			CreatedMaterialIDs.Clear();
			ChangedMaterialIDs.Clear();
		}

		public bool Any()
		{
			if (CreatedMaterialIDs.Length <= 0)
			{
				return ChangedMaterialIDs.Length > 0;
			}
			return true;
		}
	}

	public struct MaterialKey : IEquatable<MaterialKey>
	{
		public int MaterialInstanceID;

		public MaterialKey(Material material)
		{
			MaterialInstanceID = material.GetInstanceID();
		}

		public MaterialKey(int materialInstanceID)
		{
			MaterialInstanceID = materialInstanceID;
		}

		public bool Equals(MaterialKey other)
		{
			return MaterialInstanceID == other.MaterialInstanceID;
		}

		public override bool Equals(object obj)
		{
			if (obj is MaterialKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return MaterialInstanceID;
		}
	}

	public struct ManagedMaterialInfo : IGPUDrivenMemoryProfilingSource
	{
		public Material OriginalMaterial;

		public Material SubstituteMaterial;

		public GPUDrivenRenderingUtils.PropertyLayout PropertyLayout;

		public void FillMemoryCounters(Counters.CounterCollection counters)
		{
			PropertyLayout.FillMemoryCounter(counters, counters.ResourceDataCPU);
		}
	}

	public struct UnmanagedMaterialInfo
	{
		public MaterialBatchCollection BatchCollection;

		public BatchMaterialID OriginalBatchMaterialID;

		public BatchMaterialID EffectiveBatchMaterialID;

		public int OriginalMaterialInstanceID;

		public MaterialFlags Flags;

		public GPUDrivenAllocator.DataAllocation MaterialDataAllocation;

		public BitMask256 PropertyOverrideMask;

		public int ReferenceCount;

		public GPUDrivenMaterialUniquenessKey GPUDrivenMaterialUniquenessKey;

		public MaterialKey Key => new MaterialKey(OriginalMaterialInstanceID);

		public bool IsUsed()
		{
			return ReferenceCount > 0;
		}
	}

	public struct MaterialBatchCollection
	{
		public struct Batch
		{
			public BatchID ID;

			public int BuiltInPerInstanceDataSize;
		}

		public Batch Default;

		public Batch LightMaps;

		public Batch LightProbes;
	}

	public struct MeshKey : IEquatable<MeshKey>
	{
		public int MeshInstanceID;

		public int SubmeshIndex;

		public MeshKey(Mesh mesh, int submeshIndex)
		{
			MeshInstanceID = mesh.GetInstanceID();
			SubmeshIndex = submeshIndex;
		}

		public bool Equals(MeshKey other)
		{
			if (MeshInstanceID == other.MeshInstanceID)
			{
				return SubmeshIndex == other.SubmeshIndex;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is MeshKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(MeshInstanceID, SubmeshIndex);
		}
	}

	public struct ManagedMeshInfo : IGPUDrivenMemoryProfilingSource
	{
		public Mesh Mesh;

		public void FillMemoryCounters(Counters.CounterCollection counters)
		{
			ProfilerCounterValue<int> resourceDataCPU = counters.ResourceDataCPU;
			resourceDataCPU.Value += UnsafeUtility.SizeOf<IntPtr>();
		}
	}

	public struct UnmanagedMeshInfo
	{
		public int SubmeshIndex;

		public int MaxSubmeshCount;

		public uint IndexStart;

		public uint IndexCount;

		public BatchMeshID BatchMeshID;

		public int ReferenceCount;

		public Bounds Bounds;

		public bool IsUsed()
		{
			return ReferenceCount > 0;
		}
	}

	public const int kMaxSubMeshCount = 64;

	private readonly GPUDrivenBatchRendererGroup m_GPUDrivenBRG;

	private readonly GPUDrivenLightmapping m_Lightmapping;

	private readonly GPUDrivenResourcePool<MaterialKey, ManagedMaterialInfo, UnmanagedMaterialInfo> m_MaterialPool;

	private readonly GPUDrivenResourcePool<MeshKey, ManagedMeshInfo, UnmanagedMeshInfo> m_MeshPool;

	private readonly GPUDrivenPersistentData m_PersistentData;

	private readonly Dictionary<Mesh, BitMask256> m_SubmeshMasks = new Dictionary<Mesh, BitMask256>();

	private Updates m_Updates;

	public IGPUDrivenResourcePool<MeshKey> MeshPool => m_MeshPool;

	public IGPUDrivenResourcePool<MaterialKey> MaterialPool => m_MaterialPool;

	private GraphicsBufferHandle GraphicsBufferHandle => m_PersistentData.GPUPersistentInstanceData.BufferHandle;

	public GPUDrivenResourceRegistry(GPUDrivenBatchRendererGroup gpuDrivenBRG, GPUDrivenPersistentData persistentData, GPUDrivenLightmapping lightmapping)
	{
		m_GPUDrivenBRG = gpuDrivenBRG;
		m_PersistentData = persistentData;
		m_Updates = new Updates(Allocator.Persistent);
		m_MeshPool = new GPUDrivenResourcePool<MeshKey, ManagedMeshInfo, UnmanagedMeshInfo>(16);
		m_MaterialPool = new GPUDrivenResourcePool<MaterialKey, ManagedMaterialInfo, UnmanagedMaterialInfo>(16);
		m_Lightmapping = lightmapping;
	}

	public void Dispose()
	{
		foreach (GPUDrivenRegisteredResource<MaterialKey> item in m_MaterialPool)
		{
			ref UnmanagedMaterialInfo unmanagedResource = ref m_MaterialPool.GetUnmanagedResource(item.IndexAllocation);
			unmanagedResource.GPUDrivenMaterialUniquenessKey.BatchBreakingProperties.Dispose();
			m_MaterialPool.GetManagedResource(item.IndexAllocation).PropertyLayout.Dispose();
			if (unmanagedResource.EffectiveBatchMaterialID != unmanagedResource.OriginalBatchMaterialID)
			{
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedResource.EffectiveBatchMaterialID);
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedResource.OriginalBatchMaterialID);
			}
			else
			{
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedResource.OriginalBatchMaterialID);
			}
		}
		foreach (GPUDrivenRegisteredResource<MeshKey> item2 in m_MeshPool)
		{
			ref UnmanagedMeshInfo unmanagedResource2 = ref m_MeshPool.GetUnmanagedResource(item2.IndexAllocation);
			m_GPUDrivenBRG.BRG.UnregisterMesh(unmanagedResource2.BatchMeshID);
		}
		m_Updates.Dispose();
		m_MeshPool.Dispose();
		m_MaterialPool.Dispose();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.ResourceDataCPU, m_Updates.CreatedMaterialIDs);
		counters.CollectBufferSize(counters.ResourceDataCPU, m_Updates.ChangedMaterialIDs);
		m_MaterialPool.FillMemoryCounters(counters);
		m_MeshPool.FillMemoryCounters(counters);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<UnmanagedMeshInfo>.ReadOnly GetInnerUnmanagedMeshPool()
	{
		return m_MeshPool.GetInnerUnmanagedPool();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<UnmanagedMaterialInfo>.ReadOnly GetInnerUnmanagedMaterialPool()
	{
		return m_MaterialPool.GetInnerUnmanagedPool();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref UnmanagedMeshInfo GetUnmanagedMeshInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref m_MeshPool.GetUnmanagedResource(indexAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref UnmanagedMaterialInfo GetUnmanagedMaterialInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref m_MaterialPool.GetUnmanagedResource(indexAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref readonly UnmanagedMaterialInfo ReadUnmanagedMaterialInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref m_MaterialPool.ReadUnmanagedResource(indexAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref ManagedMeshInfo GetManagedMeshInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref m_MeshPool.GetManagedResource(indexAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref ManagedMaterialInfo GetManagedMaterialInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref m_MaterialPool.GetManagedResource(indexAllocation);
	}

	public bool TryGetMaterialIndexAllocation(Material material, out GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		GPUDrivenResourcePool<MaterialKey, ManagedMaterialInfo, UnmanagedMaterialInfo> materialPool = m_MaterialPool;
		MaterialKey key = new MaterialKey(material);
		return materialPool.TryGetAllocation(in key, out indexAllocation);
	}

	public bool TryGetMaterialIndexAllocation(int materialInstanceID, out GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		GPUDrivenResourcePool<MaterialKey, ManagedMaterialInfo, UnmanagedMaterialInfo> materialPool = m_MaterialPool;
		MaterialKey key = new MaterialKey(materialInstanceID);
		return materialPool.TryGetAllocation(in key, out indexAllocation);
	}

	public GPUDrivenIndexAllocator.IndexAllocation GetOrRegisterMesh(Mesh mesh, int submeshIndex)
	{
		if (mesh.subMeshCount > 64)
		{
			Debug.LogError("Submesh count exceeds the maximum.");
		}
		MeshKey key = new MeshKey(mesh, submeshIndex);
		bool isNew;
		GPUDrivenIndexAllocator.IndexAllocation orAllocate = m_MeshPool.GetOrAllocate(in key, out isNew);
		if (isNew)
		{
			ref ManagedMeshInfo managedResource = ref m_MeshPool.GetManagedResource(orAllocate);
			ref UnmanagedMeshInfo unmanagedResource = ref m_MeshPool.GetUnmanagedResource(orAllocate);
			managedResource.Mesh = mesh;
			unmanagedResource.SubmeshIndex = submeshIndex;
			unmanagedResource.MaxSubmeshCount = managedResource.Mesh.subMeshCount;
			int submesh = math.clamp(submeshIndex, 0, unmanagedResource.MaxSubmeshCount - 1);
			unmanagedResource.IndexStart = managedResource.Mesh.GetIndexStart(submesh);
			unmanagedResource.IndexCount = managedResource.Mesh.GetIndexCount(submesh);
			unmanagedResource.BatchMeshID = m_GPUDrivenBRG.BRG.RegisterMesh(mesh);
			unmanagedResource.Bounds = mesh.bounds;
			BitMask256 valueOrDefault = m_SubmeshMasks.GetValueOrDefault(mesh, default(BitMask256));
			valueOrDefault.SetBit(submeshIndex, value: true);
			m_SubmeshMasks[mesh] = valueOrDefault;
		}
		return orAllocate;
	}

	public GPUDrivenRenderingUtils.MaterialLayoutChanges UpdateMaterialLayout(ref ManagedMaterialInfo managedInfo, in UnmanagedMaterialInfo unmanagedInfo)
	{
		Shader shader = managedInfo.OriginalMaterial.shader;
		GPUDrivenShaderMetadata shaderMetadata = m_GPUDrivenBRG.ShaderMetadataRepository.Get(shader);
		return ExtractMaterialLayout(managedInfo.OriginalMaterial, ref managedInfo.PropertyLayout, in unmanagedInfo, in shaderMetadata);
	}

	public GPUDrivenIndexAllocator.IndexAllocation GetOrRegisterMaterial(Material material)
	{
		MaterialKey key = new MaterialKey(material);
		bool isNew;
		GPUDrivenIndexAllocator.IndexAllocation orAllocate = m_MaterialPool.GetOrAllocate(in key, out isNew);
		if (isNew)
		{
			int value = material.GetInstanceID();
			ref ManagedMaterialInfo managedResource = ref m_MaterialPool.GetManagedResource(orAllocate);
			ref UnmanagedMaterialInfo unmanagedResource = ref m_MaterialPool.GetUnmanagedResource(orAllocate);
			unmanagedResource.ReferenceCount++;
			managedResource.OriginalMaterial = material;
			unmanagedResource.OriginalMaterialInstanceID = value;
			unmanagedResource.EffectiveBatchMaterialID = (unmanagedResource.OriginalBatchMaterialID = m_GPUDrivenBRG.BRG.RegisterMaterial(material));
			unmanagedResource.Flags = ExtractMaterialFlags(material);
			GPUDrivenMaterialUniquenessKey existingKey = default(GPUDrivenMaterialUniquenessKey);
			unmanagedResource.GPUDrivenMaterialUniquenessKey = ExtractMaterialUniquenessKey(material, in existingKey);
			managedResource.PropertyLayout = new GPUDrivenRenderingUtils.PropertyLayout
			{
				PerMaterialData = new NativeList<GPUDrivenRenderer.PropertyData>(Allocator.Persistent),
				PerInstanceData = new NativeList<GPUDrivenRenderer.PropertyData>(Allocator.Persistent)
			};
			GPUDrivenShaderMetadata shaderMetadata = m_GPUDrivenBRG.ShaderMetadataRepository.Get(material.shader);
			ExtractMaterialLayout(material, ref managedResource.PropertyLayout, in unmanagedResource, in shaderMetadata);
			unmanagedResource.BatchCollection = CreateBatchCollection(in managedResource.PropertyLayout);
			unmanagedResource.MaterialDataAllocation = GPUDrivenAllocator.DataAllocation.Empty;
			ReallocatePerMaterialData(orAllocate, ref managedResource, ref unmanagedResource);
			unmanagedResource.ReferenceCount--;
			if (managedResource.PropertyLayout.PerMaterialDataSizeInBytes > 0 && !unmanagedResource.MaterialDataAllocation.IsValid())
			{
				m_MaterialPool.Free(in key);
				return GPUDrivenIndexAllocator.IndexAllocation.Invalid;
			}
			m_Updates.CreatedMaterialIDs.Add(in value);
			m_Updates.ChangedMaterialIDs.Add(in value);
		}
		return orAllocate;
	}

	public void ReallocatePerMaterialData(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ReallocatePerMaterialData(indexAllocation, ref m_MaterialPool.GetManagedResource(indexAllocation), ref m_MaterialPool.GetUnmanagedResource(indexAllocation));
	}

	private void ReallocatePerMaterialData(GPUDrivenIndexAllocator.IndexAllocation indexAllocation, ref ManagedMaterialInfo materialInfo, ref UnmanagedMaterialInfo unmanagedInfo)
	{
		if (unmanagedInfo.MaterialDataAllocation.IsValid())
		{
			m_PersistentData.FreeMaterialData(unmanagedInfo.MaterialDataAllocation);
		}
		if (materialInfo.PropertyLayout.PerMaterialDataSizeInBytes > 0)
		{
			unmanagedInfo.MaterialDataAllocation = m_PersistentData.AllocateMaterialData(materialInfo.PropertyLayout.PerMaterialDataSizeInBytes);
			unmanagedInfo.PropertyOverrideMask = BitMask256.FirstBitsSet(materialInfo.PropertyLayout.PerMaterialData.Length);
			if (unmanagedInfo.MaterialDataAllocation.IsValid())
			{
				m_GPUDrivenBRG.WriteDefaultMaterialData(indexAllocation);
			}
		}
		else
		{
			unmanagedInfo.MaterialDataAllocation = GPUDrivenAllocator.DataAllocation.Empty;
		}
	}

	private MaterialBatchCollection CreateBatchCollection(in GPUDrivenRenderingUtils.PropertyLayout propertyLayout)
	{
		GPUDrivenMetadataAuthoring.MaterialMetadata materialMetadata = GPUDrivenMetadataAuthoring.FromMaterialLayout(GPUDrivenMetadataAuthoring.MetadataComponents.Default, in propertyLayout, Allocator.Temp);
		MaterialBatchCollection result = default(MaterialBatchCollection);
		result.Default = new MaterialBatchCollection.Batch
		{
			ID = m_GPUDrivenBRG.BRG.AddBatch(materialMetadata.MetadataValues, GraphicsBufferHandle),
			BuiltInPerInstanceDataSize = materialMetadata.BuiltInPerInstanceDataSize
		};
		materialMetadata.Dispose();
		GPUDrivenMetadataAuthoring.MaterialMetadata materialMetadata2 = GPUDrivenMetadataAuthoring.FromMaterialLayout(GPUDrivenMetadataAuthoring.MetadataComponents.LightMaps, in propertyLayout, Allocator.Temp);
		result.LightMaps = new MaterialBatchCollection.Batch
		{
			ID = m_GPUDrivenBRG.BRG.AddBatch(materialMetadata2.MetadataValues, GraphicsBufferHandle),
			BuiltInPerInstanceDataSize = materialMetadata2.BuiltInPerInstanceDataSize
		};
		materialMetadata2.Dispose();
		GPUDrivenMetadataAuthoring.MaterialMetadata materialMetadata3 = GPUDrivenMetadataAuthoring.FromMaterialLayout(GPUDrivenMetadataAuthoring.MetadataComponents.LightProbes, in propertyLayout, Allocator.Temp);
		result.LightProbes = new MaterialBatchCollection.Batch
		{
			ID = m_GPUDrivenBRG.BRG.AddBatch(materialMetadata3.MetadataValues, GraphicsBufferHandle),
			BuiltInPerInstanceDataSize = materialMetadata3.BuiltInPerInstanceDataSize
		};
		materialMetadata3.Dispose();
		return result;
	}

	private void RemoveBatchCollection(MaterialBatchCollection materialBatchCollection)
	{
		m_GPUDrivenBRG.BRG.RemoveBatch(materialBatchCollection.Default.ID);
		m_GPUDrivenBRG.BRG.RemoveBatch(materialBatchCollection.LightMaps.ID);
		m_GPUDrivenBRG.BRG.RemoveBatch(materialBatchCollection.LightProbes.ID);
	}

	internal Updates GetResourceUpdatesAndClear(Allocator allocator)
	{
		Updates result = default(Updates);
		if (m_Updates.Any())
		{
			result.CreatedMaterialIDs = new NativeList<int>(m_Updates.CreatedMaterialIDs.Length, allocator);
			result.CreatedMaterialIDs.CopyFrom(in m_Updates.CreatedMaterialIDs);
			result.ChangedMaterialIDs = new NativeList<int>(m_Updates.ChangedMaterialIDs.Length, allocator);
			result.ChangedMaterialIDs.CopyFrom(in m_Updates.ChangedMaterialIDs);
		}
		else
		{
			result.CreatedMaterialIDs = new NativeList<int>(1, allocator);
			result.ChangedMaterialIDs = new NativeList<int>(1, allocator);
		}
		m_Updates.Clear();
		return result;
	}

	public void UpdateMeshAsset(Mesh mesh, NativeList<GPUDrivenIndexAllocator.IndexAllocation> forceUpdatedMeshAllocations)
	{
		if (!m_SubmeshMasks.TryGetValue(mesh, out var value))
		{
			return;
		}
		bool flag = false;
		foreach (int item in value)
		{
			MeshKey key = new MeshKey(mesh, item);
			if (m_MeshPool.TryGetAllocation(in key, out var indexAllocation))
			{
				ref ManagedMeshInfo managedResource = ref m_MeshPool.GetManagedResource(indexAllocation);
				ref UnmanagedMeshInfo unmanagedResource = ref m_MeshPool.GetUnmanagedResource(indexAllocation);
				bool flag2 = false;
				int subMeshCount = mesh.subMeshCount;
				if (unmanagedResource.MaxSubmeshCount != subMeshCount)
				{
					unmanagedResource.MaxSubmeshCount = subMeshCount;
					flag2 = true;
					flag = true;
				}
				int submesh = math.clamp(unmanagedResource.SubmeshIndex, 0, unmanagedResource.MaxSubmeshCount - 1);
				uint indexStart = managedResource.Mesh.GetIndexStart(submesh);
				if (unmanagedResource.IndexStart != indexStart)
				{
					unmanagedResource.IndexStart = indexStart;
					flag = true;
				}
				uint indexCount = managedResource.Mesh.GetIndexCount(submesh);
				if (unmanagedResource.IndexCount != indexCount)
				{
					unmanagedResource.IndexCount = indexCount;
					flag = true;
				}
				Bounds bounds = managedResource.Mesh.bounds;
				if (!unmanagedResource.Bounds.Equals(bounds))
				{
					unmanagedResource.Bounds = bounds;
					flag2 = true;
					flag = true;
				}
				if (flag2)
				{
					forceUpdatedMeshAllocations.Add(in indexAllocation);
				}
			}
		}
		if (flag)
		{
			m_GPUDrivenBRG.RendererGroupPool.OnModifiedCPUData();
		}
	}

	public void TryFreeMaterial(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ref UnmanagedMaterialInfo unmanagedResource = ref m_MaterialPool.GetUnmanagedResource(indexAllocation);
		if (!unmanagedResource.IsUsed())
		{
			RemoveBatchCollection(unmanagedResource.BatchCollection);
			if (unmanagedResource.EffectiveBatchMaterialID != unmanagedResource.OriginalBatchMaterialID)
			{
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedResource.EffectiveBatchMaterialID);
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedResource.OriginalBatchMaterialID);
			}
			else
			{
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedResource.OriginalBatchMaterialID);
			}
			if (unmanagedResource.MaterialDataAllocation.IsValid())
			{
				m_PersistentData.FreeMaterialData(unmanagedResource.MaterialDataAllocation);
			}
			unmanagedResource.GPUDrivenMaterialUniquenessKey.BatchBreakingProperties.Dispose();
			ref ManagedMaterialInfo managedResource = ref m_MaterialPool.GetManagedResource(indexAllocation);
			managedResource.PropertyLayout.Dispose();
			m_Lightmapping.RemoveAll(managedResource.OriginalMaterial, unmanagedResource.OriginalBatchMaterialID);
			GPUDrivenResourcePool<MaterialKey, ManagedMaterialInfo, UnmanagedMaterialInfo> materialPool = m_MaterialPool;
			MaterialKey key = unmanagedResource.Key;
			materialPool.Free(in key);
		}
	}

	public void TryFreeMesh(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ref UnmanagedMeshInfo unmanagedResource = ref m_MeshPool.GetUnmanagedResource(indexAllocation);
		if (unmanagedResource.IsUsed())
		{
			return;
		}
		m_GPUDrivenBRG.BRG.UnregisterMesh(unmanagedResource.BatchMeshID);
		ref ManagedMeshInfo managedResource = ref m_MeshPool.GetManagedResource(indexAllocation);
		if (m_SubmeshMasks.TryGetValue(managedResource.Mesh, out var value))
		{
			value.SetBit(unmanagedResource.SubmeshIndex, value: false);
			if (value.Any())
			{
				m_SubmeshMasks[managedResource.Mesh] = value;
			}
			else
			{
				m_SubmeshMasks.Remove(managedResource.Mesh);
			}
		}
		GPUDrivenResourcePool<MeshKey, ManagedMeshInfo, UnmanagedMeshInfo> meshPool = m_MeshPool;
		MeshKey key = new MeshKey(managedResource.Mesh, unmanagedResource.SubmeshIndex);
		meshPool.Free(in key);
	}

	public void SubstituteMaterialBatch(GPUDrivenIndexAllocator.IndexAllocation materialIndexAllocation, Material material)
	{
		ref ManagedMaterialInfo managedMaterialInfo = ref GetManagedMaterialInfo(materialIndexAllocation);
		if (!(managedMaterialInfo.SubstituteMaterial == material) && (!(managedMaterialInfo.SubstituteMaterial == null) || !(managedMaterialInfo.OriginalMaterial == material)))
		{
			ref UnmanagedMaterialInfo unmanagedMaterialInfo = ref GetUnmanagedMaterialInfo(materialIndexAllocation);
			RemoveBatchCollection(unmanagedMaterialInfo.BatchCollection);
			if (unmanagedMaterialInfo.EffectiveBatchMaterialID != unmanagedMaterialInfo.OriginalBatchMaterialID)
			{
				m_GPUDrivenBRG.BRG.UnregisterMaterial(unmanagedMaterialInfo.EffectiveBatchMaterialID);
			}
			managedMaterialInfo.SubstituteMaterial = material;
			unmanagedMaterialInfo.EffectiveBatchMaterialID = m_GPUDrivenBRG.BRG.RegisterMaterial(material);
			unmanagedMaterialInfo.BatchCollection = CreateBatchCollection(in managedMaterialInfo.PropertyLayout);
		}
	}

	public void RecreateAllBatchIDs()
	{
		foreach (GPUDrivenRegisteredResource<MaterialKey> item in m_MaterialPool)
		{
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = item.IndexAllocation;
			ref ManagedMaterialInfo managedMaterialInfo = ref GetManagedMaterialInfo(indexAllocation);
			ref UnmanagedMaterialInfo unmanagedMaterialInfo = ref GetUnmanagedMaterialInfo(indexAllocation);
			RemoveBatchCollection(unmanagedMaterialInfo.BatchCollection);
			unmanagedMaterialInfo.BatchCollection = CreateBatchCollection(in managedMaterialInfo.PropertyLayout);
		}
	}

	private static MaterialFlags ExtractMaterialFlags(Material material)
	{
		MaterialFlags materialFlags = MaterialFlags.None;
		if (material == null)
		{
			return materialFlags;
		}
		int renderQueue = material.renderQueue;
		RenderQueueRange transparent = RenderQueueRange.transparent;
		if (transparent.lowerBound <= renderQueue && renderQueue <= transparent.upperBound)
		{
			materialFlags |= MaterialFlags.Transparent;
		}
		RenderQueueRange opaqueDistortion = WaaaghRenderQueue.OpaqueDistortion;
		if (opaqueDistortion.lowerBound <= renderQueue && renderQueue <= opaqueDistortion.upperBound)
		{
			materialFlags |= MaterialFlags.OpaqueDistortion;
		}
		return materialFlags;
	}

	public bool TryUpdateMaterialFlags(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ref ManagedMaterialInfo managedMaterialInfo = ref GetManagedMaterialInfo(indexAllocation);
		ref UnmanagedMaterialInfo unmanagedMaterialInfo = ref GetUnmanagedMaterialInfo(indexAllocation);
		MaterialFlags materialFlags = ExtractMaterialFlags(managedMaterialInfo.OriginalMaterial);
		if (unmanagedMaterialInfo.Flags == materialFlags)
		{
			return false;
		}
		unmanagedMaterialInfo.Flags = materialFlags;
		return true;
	}

	private GPUDrivenMaterialUniquenessKey ExtractMaterialUniquenessKey(Material material, in GPUDrivenMaterialUniquenessKey existingKey = default(GPUDrivenMaterialUniquenessKey))
	{
		if (material == null)
		{
			return default(GPUDrivenMaterialUniquenessKey);
		}
		Shader shader = ((material != null) ? material.shader : null);
		if (shader == null)
		{
			return default(GPUDrivenMaterialUniquenessKey);
		}
		GPUDrivenShaderMetadata shaderMetadata = m_GPUDrivenBRG.ShaderMetadataRepository.Get(material.shader);
		GPUDrivenMaterialUniquenessKey gPUDrivenMaterialUniquenessKey = default(GPUDrivenMaterialUniquenessKey);
		gPUDrivenMaterialUniquenessKey.ShaderInstanceID = shader.GetInstanceID();
		gPUDrivenMaterialUniquenessKey.ShadowCasterPassIndex = shaderMetadata.ShadowCasterPassIndex;
		gPUDrivenMaterialUniquenessKey.DepthOnlyPassIndex = shaderMetadata.DepthOnlyPassIndex;
		gPUDrivenMaterialUniquenessKey.MotionVectorsPassIndex = shaderMetadata.MotionVectorsPassIndex;
		gPUDrivenMaterialUniquenessKey.RenderQueue = material.renderQueue;
		gPUDrivenMaterialUniquenessKey.RenderType = GPUDrivenRenderingUtils.GetRenderTypeTagValue(material);
		gPUDrivenMaterialUniquenessKey.ShaderTypeMask = shaderMetadata.TypeMask;
		GPUDrivenMaterialUniquenessKey result = gPUDrivenMaterialUniquenessKey;
		int passCount = material.passCount;
		for (int i = 0; i < passCount; i++)
		{
			string passName = material.GetPassName(i);
			if (material.GetShaderPassEnabled(passName))
			{
				result.EnabledPassesMask |= 1 << i;
			}
		}
		ref GPUDrivenShaderMetadata.BatchBreakingPropertiesMetadata batchBreakingProperties = ref shaderMetadata.BatchBreakingProperties;
		if (batchBreakingProperties.Count > 0)
		{
			result.BatchBreakingProperties = CollectBatchBreakingProperties(shader, material, in existingKey, in batchBreakingProperties, in shaderMetadata.VirtualTexturePropertyMask, Allocator.Persistent);
			result.BatchBreakingPropertiesHashCode = ComputeBreakingPropertiesHashCode(result.BatchBreakingProperties, result.IgnoredBatchBreakingProperties);
		}
		result.EnabledKeywordsMask = ConstructEnabledKeywordsMask(material, in shaderMetadata);
		result.EnabledShadowCasterKeywordsMask = result.EnabledKeywordsMask;
		result.EnabledDepthOnlyKeywordsMask = result.EnabledKeywordsMask;
		result.IgnoredShadowCasterBatchBreakingProperties = default(BitMask256);
		result.IgnoredDepthOnlyBreakingProperties = default(BitMask256);
		bool flag = material.IsKeywordEnabled("_ALPHATEST_ON");
		if (result.ShadowCasterPassIndex != -1)
		{
			if (result.ShadowCasterPassIndex >= shaderMetadata.Passes.Length)
			{
				result.ShadowCasterPassIndex = -1;
			}
			else
			{
				GPUDrivenShaderMetadata.PassMetadata passMetadata = shaderMetadata.Passes[result.ShadowCasterPassIndex];
				result.EnabledShadowCasterKeywordsMask = result.EnabledShadowCasterKeywordsMask.And(in passMetadata.LocalKeywordsMask);
				result.IgnoredShadowCasterBatchBreakingProperties = passMetadata.IgnoredBatchBreakingProperties;
				if (!flag)
				{
					result.IgnoredShadowCasterBatchBreakingProperties = result.IgnoredShadowCasterBatchBreakingProperties.Or(in passMetadata.IgnoredBreakingPropertiesNoAlphaTest);
				}
				result.ShadowCasterBreakingPropertiesHashCode = ComputeBreakingPropertiesHashCode(result.BatchBreakingProperties, result.IgnoredShadowCasterBatchBreakingProperties);
			}
		}
		if (result.DepthOnlyPassIndex != -1)
		{
			if (result.DepthOnlyPassIndex >= shaderMetadata.Passes.Length)
			{
				result.DepthOnlyPassIndex = -1;
			}
			else
			{
				GPUDrivenShaderMetadata.PassMetadata passMetadata2 = shaderMetadata.Passes[result.DepthOnlyPassIndex];
				result.EnabledDepthOnlyKeywordsMask = result.EnabledDepthOnlyKeywordsMask.And(in passMetadata2.LocalKeywordsMask);
				result.IgnoredDepthOnlyBreakingProperties = passMetadata2.IgnoredBatchBreakingProperties;
				if (!flag)
				{
					result.IgnoredDepthOnlyBreakingProperties = result.IgnoredDepthOnlyBreakingProperties.Or(in passMetadata2.IgnoredBreakingPropertiesNoAlphaTest);
				}
				result.DepthOnlyBreakingPropertiesHashCode = ComputeBreakingPropertiesHashCode(result.BatchBreakingProperties, result.IgnoredDepthOnlyBreakingProperties);
			}
		}
		return result;
	}

	private static int ComputeBreakingPropertiesHashCode(UnsafeList<GPUDrivenBatchBreakingProperty> breakingProperties, BitMask256 ignoredBreakingProperties)
	{
		int num = 0;
		foreach (GPUDrivenBatchBreakingProperty item in breakingProperties)
		{
			if (!ignoredBreakingProperties.GetBit(item.PropertyIndex))
			{
				num = HashCode.Combine(num, item);
			}
		}
		return num;
	}

	private static BitMask256 ConstructEnabledKeywordsMask(Material material, in GPUDrivenShaderMetadata shaderMetadata)
	{
		BitMask256 result = default(BitMask256);
		if (shaderMetadata.LocalKeywords != null)
		{
			for (int i = 0; i < shaderMetadata.LocalKeywords.Length; i++)
			{
				LocalKeyword keyword = shaderMetadata.LocalKeywords[i];
				if (material.IsKeywordEnabled(in keyword))
				{
					result.SetBit(i, value: true);
				}
			}
		}
		return result;
	}

	private static UnsafeList<GPUDrivenBatchBreakingProperty> CollectBatchBreakingProperties(Shader shader, Material material, in GPUDrivenMaterialUniquenessKey existingKey, in GPUDrivenShaderMetadata.BatchBreakingPropertiesMetadata batchBreakingPropertiesMetadata, in BitMask256 virtualTexturePropertyMask, Allocator allocator)
	{
		UnsafeList<GPUDrivenBatchBreakingProperty> batchBreakingProperties = existingKey.BatchBreakingProperties;
		UnsafeList<GPUDrivenBatchBreakingProperty> result;
		if (batchBreakingProperties.IsCreated && batchBreakingProperties.Length == batchBreakingPropertiesMetadata.Count)
		{
			result = batchBreakingProperties;
		}
		else
		{
			batchBreakingProperties.Dispose();
			result = new UnsafeList<GPUDrivenBatchBreakingProperty>(batchBreakingPropertiesMetadata.Count, allocator);
			result.Length = batchBreakingPropertiesMetadata.Count;
		}
		int num = 0;
		bool flag = WaaaghPipeline.Asset.VirtualTextureSettings.Enabled && VirtualTextureUtils.DoesMaterialUseVT(material);
		foreach (int item in batchBreakingPropertiesMetadata.Mask)
		{
			if (!flag || !virtualTexturePropertyMask.GetBit(item))
			{
				GPUDrivenBatchBreakingProperty value = GPUDrivenBatchBreakingProperty.Extract(shader, material, item);
				result[num] = value;
				num++;
			}
		}
		result.Length = num;
		return result;
	}

	public void UpdateMaterialUniquenessKey(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		ref ManagedMaterialInfo managedMaterialInfo = ref GetManagedMaterialInfo(indexAllocation);
		ref UnmanagedMaterialInfo unmanagedMaterialInfo = ref GetUnmanagedMaterialInfo(indexAllocation);
		unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey = ExtractMaterialUniquenessKey(managedMaterialInfo.OriginalMaterial, in unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey);
	}

	private GPUDrivenRenderingUtils.MaterialLayoutChanges ExtractMaterialLayout(Material material, ref GPUDrivenRenderingUtils.PropertyLayout propertyLayout, in UnmanagedMaterialInfo unmanagedMaterialInfo, in GPUDrivenShaderMetadata shaderMetadata)
	{
		GPUDrivenRenderingUtils.MaterialLayoutChanges materialLayoutChanges = default(GPUDrivenRenderingUtils.MaterialLayoutChanges);
		materialLayoutChanges.PerInstanceData = GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Nothing;
		materialLayoutChanges.PerMaterialData = GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Nothing;
		GPUDrivenRenderingUtils.MaterialLayoutChanges result = materialLayoutChanges;
		int perInstanceDataSizeInBytes = propertyLayout.PerInstanceDataSizeInBytes;
		int perMaterialDataSizeInBytes = propertyLayout.PerMaterialDataSizeInBytes;
		propertyLayout.PerInstanceDataSizeInBytes = 0;
		propertyLayout.PerMaterialDataSizeInBytes = 0;
		int num = 0;
		int num2 = 0;
		Shader shader = material.shader;
		NativeArray<GPUDrivenShaderMetadataRepository.ShaderPropertyData>.ReadOnly propertyLayout2 = m_GPUDrivenBRG.ShaderMetadataRepository.GetPropertyLayout(shader);
		List<GPUDrivenShaderMetadataRepository.ShaderPropertyData> value;
		using (ListPool<GPUDrivenShaderMetadataRepository.ShaderPropertyData>.Get(out value))
		{
			foreach (GPUDrivenShaderMetadataRepository.ShaderPropertyData item in propertyLayout2)
			{
				GPUDrivenShaderMetadataRepository.ShaderPropertyData current = item;
				if (current.TilingOffsetTextureNameID.HasValue)
				{
					int value2 = current.TilingOffsetTextureNameID.Value;
					current.PropertyData.Value.Vector = GPUDrivenRenderingUtils.GetTilingOffset(material, value2);
				}
				else
				{
					int nameID = current.PropertyData.NameID;
					switch (current.PropertyData.Type)
					{
					case GPUDrivenRenderer.PropertyDataType.Float:
						current.PropertyData.Value.Float = material.GetFloat(nameID);
						break;
					case GPUDrivenRenderer.PropertyDataType.Int:
						current.PropertyData.Value.Int = material.GetInt(nameID);
						break;
					case GPUDrivenRenderer.PropertyDataType.Vector:
						current.PropertyData.Value.Vector = material.GetVector(nameID);
						break;
					case GPUDrivenRenderer.PropertyDataType.Color:
						current.PropertyData.Value.Color = material.GetColor(nameID).linear;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				value.Add(current);
			}
			GPUDrivenXPBDIntegration.AddProperties(material, value);
			GPUDrivenIndirectRenderingIntegration.AddProperties(material, value);
			GPUDrivenVirtualTextureIntegration.AddProperties(material, value);
			foreach (GPUDrivenShaderMetadataRepository.ShaderPropertyData item2 in value)
			{
				NativeList<GPUDrivenRenderer.PropertyData> nativeList;
				int num3;
				GPUDrivenRenderingUtils.MaterialLayoutChangeFlags materialLayoutChangeFlags;
				BitMask256 bitMask;
				bool flag;
				if (item2.PerInstance)
				{
					nativeList = propertyLayout.PerInstanceData;
					num3 = num;
					materialLayoutChangeFlags = result.PerInstanceData;
					bitMask = result.DirtyPerInstanceDataMask;
					flag = true;
				}
				else
				{
					nativeList = propertyLayout.PerMaterialData;
					num3 = num2;
					materialLayoutChangeFlags = result.PerMaterialData;
					bitMask = result.DirtyPerMaterialDataMask;
					flag = false;
				}
				GPUDrivenRenderer.PropertyData data = item2.PropertyData;
				if (num3 < nativeList.Length)
				{
					GPUDrivenRenderer.PropertyData data2 = nativeList[num3];
					if (data.NameID != data2.NameID || data.Type != data2.Type)
					{
						materialLayoutChangeFlags |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.DefaultValues | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Properties | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges;
						bitMask.SetBit(num3, value: true);
					}
					else if (!GPUDrivenRenderer.PropertyData.AreStoredValuesEqual(in data, in data2))
					{
						materialLayoutChangeFlags |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.DefaultValues;
						bitMask.SetBit(num3, value: true);
						if (item2.BreaksBatching)
						{
							materialLayoutChangeFlags |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges;
						}
					}
					nativeList[num3] = data;
				}
				else
				{
					materialLayoutChangeFlags |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.DefaultValues | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.Properties | GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges;
					bitMask.SetBit(num3, value: true);
					nativeList.Add(in data);
				}
				GPUDrivenRenderer.PropertyDataType type = item2.PropertyData.Type;
				if (flag)
				{
					propertyLayout.PerInstanceDataSizeInBytes += type.GetPropertyDataSize();
					num++;
					result.PerInstanceData = materialLayoutChangeFlags;
					result.DirtyPerInstanceDataMask = bitMask;
				}
				else
				{
					propertyLayout.PerMaterialDataSizeInBytes += type.GetPropertyDataSize();
					num2++;
					result.PerMaterialData = materialLayoutChangeFlags;
					result.DirtyPerMaterialDataMask = bitMask;
				}
			}
		}
		while (num < propertyLayout.PerInstanceData.Length)
		{
			int num4 = propertyLayout.PerInstanceData.Length - 1;
			result.DirtyPerInstanceDataMask.SetBit(num4, value: true);
			propertyLayout.PerInstanceData.RemoveAt(num4);
		}
		while (num2 < propertyLayout.PerMaterialData.Length)
		{
			int num5 = propertyLayout.PerMaterialData.Length - 1;
			result.DirtyPerMaterialDataMask.SetBit(num5, value: true);
			propertyLayout.PerMaterialData.RemoveAt(num5);
		}
		if (propertyLayout.PerInstanceDataSizeInBytes > perInstanceDataSizeInBytes)
		{
			result.PerInstanceData |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.TotalSizeIncreased;
		}
		if (propertyLayout.PerMaterialDataSizeInBytes > perMaterialDataSizeInBytes && (!unmanagedMaterialInfo.MaterialDataAllocation.IsValid() || propertyLayout.PerMaterialDataSizeInBytes > unmanagedMaterialInfo.MaterialDataAllocation.Allocation.Size))
		{
			result.PerMaterialData |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.TotalSizeIncreased;
		}
		if ((result.PerMaterialData & GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges) == 0 && (material.renderQueue != unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey.RenderQueue || GPUDrivenRenderingUtils.GetRenderTypeTagValue(material) != unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey.RenderType))
		{
			result.PerMaterialData |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges;
		}
		if ((result.PerMaterialData & GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges) == 0 && !ConstructEnabledKeywordsMask(material, in shaderMetadata).Equals(unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey.EnabledKeywordsMask))
		{
			result.PerMaterialData |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges;
		}
		if ((result.PerMaterialData & GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges) == 0 && !AreBatchBreakingPropertiesConsistent(in shaderMetadata.BatchBreakingProperties, in unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey, material, shader))
		{
			result.PerMaterialData |= GPUDrivenRenderingUtils.MaterialLayoutChangeFlags.BatchBreakingChanges;
		}
		return result;
	}

	private static bool AreBatchBreakingPropertiesConsistent(in GPUDrivenShaderMetadata.BatchBreakingPropertiesMetadata batchBreakingPropertiesMetadata, in GPUDrivenMaterialUniquenessKey materialUniquenessKey, Material material, Shader shader)
	{
		int breakingPropertiesCount = materialUniquenessKey.GetBreakingPropertiesCount();
		if (batchBreakingPropertiesMetadata.Count != breakingPropertiesCount)
		{
			return false;
		}
		if (breakingPropertiesCount == 0)
		{
			return false;
		}
		int num = 0;
		foreach (int item in batchBreakingPropertiesMetadata.Mask)
		{
			if (!GPUDrivenBatchBreakingProperty.Extract(shader, material, item).Equals(materialUniquenessKey.BatchBreakingProperties[num]))
			{
				return false;
			}
			num++;
		}
		return true;
	}
}
