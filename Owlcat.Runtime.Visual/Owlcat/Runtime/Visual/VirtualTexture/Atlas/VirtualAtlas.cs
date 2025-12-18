using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;
using Owlcat.Runtime.Visual.VirtualTexture.Materials;
using Owlcat.Runtime.Visual.VirtualTexture.TiledTexture;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

public class VirtualAtlas : IDisposable
{
	private const int kMaxMipBias = 16383;

	private readonly int2 m_DefaultAtlasSize;

	private readonly AllocatorOptions m_DefaultAllocatorOptions;

	private AtlasAllocator m_AtlasAllocator;

	private NativeList<VirtualAtlasEntry> m_Entries;

	private NativeParallelHashMap<TextureStackId, int> m_TextureStackIndices;

	private NativeParallelHashMap<int, MaterialStackIndices> m_MaterialStackIndices;

	private NativeArray<Page> m_Pages;

	private NativeParallelHashMap<int3, int2> m_PhysicalToVirtualPageMap;

	private GraphicsBuffer m_TextureStackDataBuffer;

	private NativeList<EntryToAllocate> m_DataToAllocate;

	private NativeParallelHashSet<MaterialStackIdMapping> m_InvalidMaterialStacks;

	private NativeReference<int> m_MaxMipCountRef;

	private int2 m_ResolutionInTiles;

	private Action<int2> m_ResolutionChanged;

	private Action m_MaterialsChanged;

	public AtlasAllocator AtlasAllocator => m_AtlasAllocator;

	public NativeList<VirtualAtlasEntry> Entries => m_Entries;

	public NativeArray<Page> Pages => m_Pages;

	public int2 ResolutionInTiles => m_ResolutionInTiles;

	public int MipCount => m_MaxMipCountRef.Value;

	public GraphicsBuffer TextureStackDataBuffer => m_TextureStackDataBuffer;

	public bool IsEmpty
	{
		get
		{
			if (m_ResolutionInTiles.x > 0)
			{
				return m_ResolutionInTiles.y <= 0;
			}
			return true;
		}
	}

	public float Occupancy => m_AtlasAllocator.Occupancy();

	public NativeParallelHashMap<int, MaterialStackIndices> MaterialStackIndices => m_MaterialStackIndices;

	public NativeParallelHashMap<TextureStackId, int> TextureStackIndices => m_TextureStackIndices;

	public NativeParallelHashMap<int3, int2> PhysicalToVirtualPageMap => m_PhysicalToVirtualPageMap;

	public VirtualAtlas(PhysicalAtlasResolution physicalAtlasResolution, Action<int2> resolutionChangedCallback, Action materialChangedCallback)
		: this(new int2(256, 64), physicalAtlasResolution, resolutionChangedCallback, materialChangedCallback)
	{
	}

	public VirtualAtlas(int2 defaultSize, PhysicalAtlasResolution physicalAtlasResolution, Action<int2> resolutionChangedCallback, Action materialsChangedCallback)
	{
		m_DefaultAtlasSize = defaultSize;
		m_DefaultAllocatorOptions = new AllocatorOptions
		{
			Alignment = 1,
			SmallSizeThreshold = 8,
			LargeSizeThreshold = 24
		};
		m_AtlasAllocator = new AtlasAllocator(m_DefaultAtlasSize, in m_DefaultAllocatorOptions);
		m_Entries = new NativeList<VirtualAtlasEntry>(16, Allocator.Persistent);
		m_TextureStackIndices = new NativeParallelHashMap<TextureStackId, int>(16, Allocator.Persistent);
		m_MaterialStackIndices = new NativeParallelHashMap<int, MaterialStackIndices>(16, Allocator.Persistent);
		m_PhysicalToVirtualPageMap = new NativeParallelHashMap<int3, int2>(physicalAtlasResolution.TotalTiles(), Allocator.Persistent);
		m_DataToAllocate = new NativeList<EntryToAllocate>(16, Allocator.Persistent);
		m_InvalidMaterialStacks = new NativeParallelHashSet<MaterialStackIdMapping>(16, Allocator.Persistent);
		InitPages();
		m_MaxMipCountRef = new NativeReference<int>(Allocator.Persistent);
		UpdateResolution();
		m_ResolutionChanged = resolutionChangedCallback;
		m_MaterialsChanged = materialsChangedCallback;
	}

	private void InitPages()
	{
		m_Pages = new NativeArray<Page>(m_AtlasAllocator.Width * m_AtlasAllocator.Height, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Span<Page> span = m_Pages.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref Page reference = ref span[i];
			reference.TextureId = -1;
			reference.FrameId = -1;
			reference.PhysicalTileCoord = -1;
			reference.IsLoading = false;
			reference.MipLevel = 0;
		}
	}

	private void UpdateResolution()
	{
		if (m_ResolutionInTiles.x != m_AtlasAllocator.Width || m_ResolutionInTiles.y != m_AtlasAllocator.Height)
		{
			m_ResolutionInTiles = new int2(m_AtlasAllocator.Width, m_AtlasAllocator.Height);
			m_ResolutionChanged?.Invoke(m_ResolutionInTiles);
		}
	}

	public void Dispose()
	{
		m_AtlasAllocator.Dispose();
		if (m_Entries.IsCreated)
		{
			m_Entries.Dispose();
		}
		if (m_TextureStackIndices.IsCreated)
		{
			m_TextureStackIndices.Dispose();
		}
		if (m_MaterialStackIndices.IsCreated)
		{
			m_MaterialStackIndices.Dispose();
		}
		if (m_DataToAllocate.IsCreated)
		{
			m_DataToAllocate.Dispose();
		}
		if (m_InvalidMaterialStacks.IsCreated)
		{
			m_InvalidMaterialStacks.Dispose();
		}
		if (m_Pages.IsCreated)
		{
			m_Pages.Dispose();
		}
		if (m_MaxMipCountRef.IsCreated)
		{
			m_MaxMipCountRef.Dispose();
		}
		if (m_PhysicalToVirtualPageMap.IsCreated)
		{
			m_PhysicalToVirtualPageMap.Dispose();
		}
		m_TextureStackDataBuffer?.Dispose();
	}

	private void BeginUpdateMaterials()
	{
		m_DataToAllocate.Clear();
		m_InvalidMaterialStacks.Clear();
	}

	public void UpdateMaterial(Material material)
	{
		BeginUpdateMaterials();
		AddChangedMaterial(material);
		EndUpdateMaterials();
	}

	public void UpdateMaterials(List<Material> materialsToUpdate)
	{
		BeginUpdateMaterials();
		for (int i = 0; i < materialsToUpdate.Count; i++)
		{
			Material material = materialsToUpdate[i];
			AddChangedMaterial(material);
		}
		EndUpdateMaterials();
	}

	internal void UpdateMaterials(HashSet<Material> changedMaterials)
	{
		BeginUpdateMaterials();
		foreach (Material changedMaterial in changedMaterials)
		{
			AddChangedMaterial(changedMaterial);
		}
		EndUpdateMaterials();
	}

	private void AddChangedMaterial(Material material)
	{
		int instanceID = material.GetInstanceID();
		int materialStackCount = VirtualTextureUtils.GetMaterialStackCount(material);
		if (materialStackCount > 0)
		{
			MaterialStackIndices item;
			bool flag = m_MaterialStackIndices.TryGetValue(instanceID, out item);
			item.Count = materialStackCount;
			m_MaterialStackIndices[instanceID] = item;
			for (int i = 0; i < materialStackCount; i++)
			{
				TextureStackId stackId = CreateTextureStackId(material, i);
				if (!VirtualTextureUtils.ValidateTextureStackId(ref stackId))
				{
					stackId[0] = TiledTextureDB.kErrorGuid;
					stackId[1] = Guid.Empty;
					stackId[2] = Guid.Empty;
					stackId[3] = Guid.Empty;
				}
				int item2;
				bool flag2 = m_TextureStackIndices.TryGetValue(stackId, out item2);
				bool flag3 = item[i] == item2;
				if (!flag2)
				{
					ref NativeList<EntryToAllocate> dataToAllocate = ref m_DataToAllocate;
					EntryToAllocate value = CreateAllocationData(in stackId);
					dataToAllocate.Add(in value);
					m_TextureStackIndices.Add(stackId, -1);
				}
				if (!flag || !flag2 || !flag3)
				{
					m_InvalidMaterialStacks.Add(new MaterialStackIdMapping
					{
						MaterialId = instanceID,
						IndexInMaterial = i,
						StackId = stackId
					});
				}
			}
		}
		else
		{
			Debug.LogWarning("Material " + material.name + " has " + VirtualTextureUtils.UseOwlcatVT + " tag but doesn't have any TextureStack.", material);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private EntryToAllocate CreateAllocationData(in TextureStackId stackId)
	{
		VirtualAtlasEntry entry = CreateEntry(in stackId);
		EntryToAllocate result = default(EntryToAllocate);
		result.Rect = entry.RectInTiles;
		result.Entry = entry;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private VirtualAtlasEntry CreateEntry(in TextureStackId stackId)
	{
		VirtualAtlasEntry result = default(VirtualAtlasEntry);
		uint num = 0u;
		int layerCount = 0;
		int2 @int = 0;
		for (int i = 0; i < 4; i++)
		{
			Guid guid = stackId[i];
			if (TiledTextureDB.TryGetAssetPath(guid, out var assetPath))
			{
				TiledTextureFileHeader header = TiledTextureDB.GetHeader(assetPath);
				int num2 = header.MipCount;
				int x = header.Width;
				int y = header.Height;
				if (header.MipCount < 1 || header.Width < 128 || header.Height < 128 || header.Width % 128 > 0 || header.Height % 128 > 0)
				{
					Debug.LogError("[Virtual Texture] .tiledtexture file " + assetPath + " is invalid. Texture will be replaced by ErrorTex.");
					result.ValidGuids[layerCount++] = TiledTextureDB.kErrorGuid;
					num2 = 1;
					x = 128;
					y = 128;
				}
				else
				{
					result.ValidGuids[layerCount++] = guid;
				}
				num |= (uint)((int)header.Flags << i * 2);
				num |= (uint)(1 << 8 + i);
				if (result.MipCount == 0)
				{
					result.MipCount = num2;
					result.MipBias = header.MipBias;
				}
				else
				{
					result.MipCount = math.min(result.MipCount, num2);
					result.MipBias = math.min(result.MipBias, header.MipBias);
				}
				int2 int2 = new int2(x, y);
				@int = ((!math.all(@int == 0)) ? math.min(@int, int2) : int2);
			}
		}
		result.StackId = stackId;
		result.PackedLayerFlags = num;
		result.LayerCount = layerCount;
		result.SetMipBiasPow2(result.MipBias);
		result.TextureSizeInTiles = @int / 128;
		int2 textureSizeInTiles = result.TextureSizeInTiles;
		if (result.MipCount > 1)
		{
			textureSizeInTiles.x = (int)((float)textureSizeInTiles.x * 1.5f);
		}
		if (textureSizeInTiles.x == 0 || textureSizeInTiles.y == 0)
		{
			throw new ArgumentException("rectSizeInTiles");
		}
		result.RectInTiles.xy = 0;
		result.RectInTiles.zw = textureSizeInTiles;
		return result;
	}

	private TextureStackId CreateTextureStackId(Material material, int stackIndex)
	{
		TextureStackId result = default(TextureStackId);
		for (int i = 0; i < 4; i++)
		{
			if (!Guid.TryParse(material.GetTag(string.Format(VirtualTextureUtils.StackIdLayerIdFormat, stackIndex, i), searchFallbacks: false), out var result2))
			{
				result2 = default(Guid);
			}
			result[i] = result2;
		}
		if (result.IsEmpty())
		{
			result[0] = TiledTextureDB.kErrorGuid;
		}
		return result;
	}

	private unsafe void EndUpdateMaterials()
	{
		JobHandle jobHandle = default(JobHandle);
		if (m_DataToAllocate.Length > 1)
		{
			jobHandle = m_DataToAllocate.SortJob(default(AllocationDataComparer)).Schedule(jobHandle);
		}
		EntryAllocationJob jobData = default(EntryAllocationJob);
		jobData.Allocator = m_AtlasAllocator.GetAllocatorPtr();
		jobData.DataToAllocate = m_DataToAllocate;
		jobData.AllowGrowing = true;
		jobData.GrowStrategy = GrowStrategy.Vertically;
		jobHandle = jobData.Schedule(jobHandle);
		UpdateAtlasJob updateAtlasJob = default(UpdateAtlasJob);
		updateAtlasJob.AtlasEntries = m_Entries;
		updateAtlasJob.InvalidMaterialStackIdMappings = m_InvalidMaterialStacks;
		updateAtlasJob.DataToAllocate = m_DataToAllocate;
		updateAtlasJob.MaterialStackIndices = m_MaterialStackIndices;
		updateAtlasJob.TextureStackIndices = m_TextureStackIndices;
		updateAtlasJob.Allocator = m_AtlasAllocator.GetAllocatorPtr();
		UpdateAtlasJob jobData2 = updateAtlasJob;
		IJobExtensions.ScheduleByRef(ref jobData2, jobHandle).Complete();
		int length = Pages.Length;
		if (m_AtlasAllocator.Width * m_AtlasAllocator.Height > length)
		{
			ArrayExtensions.ResizeArray(ref m_Pages, m_AtlasAllocator.Width * m_AtlasAllocator.Height);
		}
		jobHandle = default(JobHandle);
		RemoveUnusedTextureStacksJob removeUnusedTextureStacksJob = default(RemoveUnusedTextureStacksJob);
		removeUnusedTextureStacksJob.Allocator = m_AtlasAllocator.GetAllocatorPtr();
		removeUnusedTextureStacksJob.MaterialStackIndices = m_MaterialStackIndices;
		removeUnusedTextureStacksJob.TextureStackIndices = m_TextureStackIndices;
		removeUnusedTextureStacksJob.Pages = m_Pages;
		removeUnusedTextureStacksJob.PhysicalToVirtualPageMap = m_PhysicalToVirtualPageMap;
		removeUnusedTextureStacksJob.Entries = m_Entries;
		RemoveUnusedTextureStacksJob jobData3 = removeUnusedTextureStacksJob;
		jobHandle = IJobExtensions.ScheduleByRef(ref jobData3, jobHandle);
		FindMaxMipCountJob findMaxMipCountJob = default(FindMaxMipCountJob);
		findMaxMipCountJob.MaxMipCount = m_MaxMipCountRef;
		findMaxMipCountJob.Entries = m_Entries;
		FindMaxMipCountJob jobData4 = findMaxMipCountJob;
		jobHandle = IJobExtensions.ScheduleByRef(ref jobData4, jobHandle);
		InvalidatePagesJob invalidatePagesJob = default(InvalidatePagesJob);
		invalidatePagesJob.PrevPagesCount = length;
		invalidatePagesJob.AtlasWidth = m_AtlasAllocator.Width;
		invalidatePagesJob.Pages = m_Pages;
		invalidatePagesJob.DataToAllocate = m_DataToAllocate;
		InvalidatePagesJob jobData5 = invalidatePagesJob;
		IJobExtensions.ScheduleByRef(ref jobData5, jobHandle).Complete();
		BuildTextureStackDataBuffer();
		UpdateResolution();
		m_MaterialsChanged?.Invoke();
	}

	private void BuildTextureStackDataBuffer()
	{
		if (m_TextureStackDataBuffer == null)
		{
			m_TextureStackDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_AtlasAllocator.Nodes.Length * 4, 4);
		}
		else if (m_TextureStackDataBuffer.count < m_AtlasAllocator.Nodes.Length * 4)
		{
			m_TextureStackDataBuffer.Dispose();
			m_TextureStackDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_AtlasAllocator.Nodes.Length * 4, 4);
		}
		NativeArray<uint> data = new NativeArray<uint>(m_TextureStackDataBuffer.count, Allocator.Temp);
		Span<VirtualAtlasEntry> span = m_Entries.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref VirtualAtlasEntry reference = ref span[i];
			if (reference.NodeKind == NodeKind.Alloc)
			{
				int num = i * 4;
				uint4 x = math.asuint(reference.RectInTiles);
				x = math.min(x, 65535u);
				uint2 @uint = new uint2(x.x | (x.y << 16), x.z | (x.w << 16));
				data[num++] = @uint.x;
				data[num++] = @uint.y;
				int num2 = reference.MipCount - 1;
				uint num3 = (uint)(reference.MipBias / 16383f * 32767f + 32767f);
				uint value = (uint)num2 | (num3 << 16);
				data[num++] = value;
				uint num4 = 1u;
				if (x.z > 1)
				{
					num4 = (uint)((float)x.z / 1.5f);
				}
				uint value2 = (uint)reference.LayerCount | (reference.PackedLayerFlags << 4) | (num4 << 16);
				data[num++] = value2;
			}
		}
		m_TextureStackDataBuffer.SetData(data);
		data.Dispose();
	}

	public void RemoveMaterial(int materialId)
	{
		m_MaterialStackIndices.Remove(materialId);
		RemoveUnusedTextureStacks();
	}

	public void RemoveMaterials(List<int> materialsToRemove)
	{
		for (int i = 0; i < materialsToRemove.Count; i++)
		{
			int key = materialsToRemove[i];
			m_MaterialStackIndices.Remove(key);
		}
		RemoveUnusedTextureStacks();
	}

	internal void RemoveMaterials(HashSet<int> destroyedMaterials)
	{
		foreach (int destroyedMaterial in destroyedMaterials)
		{
			m_MaterialStackIndices.Remove(destroyedMaterial);
		}
		RemoveUnusedTextureStacks();
	}

	private unsafe void RemoveUnusedTextureStacks()
	{
		RemoveUnusedTextureStacksJob removeUnusedTextureStacksJob = default(RemoveUnusedTextureStacksJob);
		removeUnusedTextureStacksJob.Allocator = m_AtlasAllocator.GetAllocatorPtr();
		removeUnusedTextureStacksJob.MaterialStackIndices = m_MaterialStackIndices;
		removeUnusedTextureStacksJob.TextureStackIndices = m_TextureStackIndices;
		removeUnusedTextureStacksJob.Pages = m_Pages;
		removeUnusedTextureStacksJob.PhysicalToVirtualPageMap = m_PhysicalToVirtualPageMap;
		removeUnusedTextureStacksJob.Entries = m_Entries;
		RemoveUnusedTextureStacksJob jobData = removeUnusedTextureStacksJob;
		IJobExtensions.ScheduleByRef(ref jobData).Complete();
	}

	public bool HasMaterial(int materialId)
	{
		return m_MaterialStackIndices.ContainsKey(materialId);
	}

	internal void Reset()
	{
		InvalidateAllPages();
		m_PhysicalToVirtualPageMap.Clear();
	}

	private void InvalidateAllPages()
	{
		Span<Page> span = m_Pages.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			span[i].ResetTileIndex();
		}
	}

	public void InvalidatePage(in int3 id)
	{
		if (m_PhysicalToVirtualPageMap.TryGetValue(id, out var item))
		{
			int index = item.y * m_ResolutionInTiles.x + item.x;
			UnsafeCollectionExtensions.ElementAsRef(in m_Pages, index).ResetTileIndex();
			m_PhysicalToVirtualPageMap.Remove(id);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void InvalidatePages(ref VirtualAtlasEntry entry, NativeArray<Page> pages, NativeParallelHashMap<int3, int2> physicalToVirtualPageMap, int atlasWidth)
	{
		for (int i = 0; i < entry.RectInTiles.w; i++)
		{
			for (int j = 0; j < entry.RectInTiles.z; j++)
			{
				int num = entry.RectInTiles.x + j;
				int index = (entry.RectInTiles.y + i) * atlasWidth + num;
				ref Page reference = ref UnsafeCollectionExtensions.ElementAsRef(in pages, index);
				reference.TextureId = -1;
				if (reference.IsReady)
				{
					physicalToVirtualPageMap.Remove(reference.PhysicalTileCoord);
				}
				reference.ResetTileIndex();
			}
		}
	}
}
