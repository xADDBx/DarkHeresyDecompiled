using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Feedback;
using Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;
using Owlcat.Runtime.Visual.VirtualTexture.Materials;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender;
using Owlcat.Runtime.Visual.VirtualTexture.Profiling;
using Owlcat.Runtime.Visual.VirtualTexture.Streaming;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture;

public class VirtualTextureManager
{
	private enum MaterialUpdateType
	{
		OnChange,
		Forced
	}

	private struct MaterialUpdateContext
	{
		public bool WriteToBRG;
	}

	public const string kVTAttributeName = "VirtualTexture(";

	private const int kClearFeedbackKernel = 0;

	private const int kPackFeedbackKernel = 1;

	private readonly GPUDrivenBatchRendererGroup m_BRG;

	private readonly FeedbackBuffer m_FeedbackBuffer;

	private int m_FrameId;

	private readonly IndirectionTextureRenderer m_IndirectTextureRenderer;

	private readonly MaterialTracker m_MaterialTracker;

	private readonly MaterialMetadataRepository m_MaterialMetadataRepository;

	private readonly PhysicalAtlas m_PhysicalAtlas;

	private readonly AsyncContext m_AsyncContext;

	private readonly PostRenderWorker m_PostRenderWorker;

	private readonly PipelineRuntimeResources m_Resources;

	private readonly VirtualTextureSettings m_Settings;

	private Matrix4x4 m_StackIdMtx = Matrix4x4.zero;

	private NativeList<GPUDrivenRenderer.PropertyData> m_TmpPropertyList;

	private VirtualAtlas m_VirtualAtlas;

	private HashSet<Material> m_ChangedMaterials;

	private HashSet<int> m_DestroyedMaterials;

	internal static Action NeedUpdatePreviews;

	internal static Action OnTileLoaderSync;

	public Texture2DArray CacheTex => m_PhysicalAtlas.AtlasTex;

	public ref readonly PhysicalAtlasResolution PhysicalAtlasResolution => ref m_AsyncContext.PhysicalAtlas.Resolution;

	public int PhysicalAtlasRequestedSizeInMegaBytes => m_PhysicalAtlas.RequestedSizeInMegaBytes;

	public int VirtualAtlasMipCount => m_AsyncContext.VirtualAtlas.MipCount;

	public RTHandle IndirectTexture => m_IndirectTextureRenderer.IndirectTexture;

	public int2 VirtualAtlasResolutionInTiles => m_VirtualAtlas.ResolutionInTiles;

	public bool IsVirtualAtlasEmpty => m_VirtualAtlas.IsEmpty;

	public GraphicsBuffer PackedFeedbackBufferUAV => m_FeedbackBuffer.PackedFeedbackBufferUAV;

	public RTHandle FeedbackRT => m_FeedbackBuffer.FeedbackRT;

	public FeedbackConsumptionTracker FeedbackConsumptionTracker { get; }

	public float VirtualAtlasOccupancy => m_VirtualAtlas.Occupancy;

	public bool VTEnabledGlobal => m_Settings?.Enabled ?? false;

	internal AtlasAllocator AtlasAllocator => m_VirtualAtlas.AtlasAllocator;

	internal int TextureStacksInAtlasCount => m_VirtualAtlas.MaterialStackIndices.Count();

	public static string TiledTexturesPath { get; } = Path.Combine(Application.streamingAssetsPath, "VirtualTexture/TiledTextures");


	public static bool LazyTileExtractionEnabled => false;

	public MaterialMetadataRepository MaterialMetadataRepository => m_MaterialMetadataRepository;

	public TileUploader TileUploader => m_PostRenderWorker.TileUploader;

	public VirtualTextureManager(VirtualTextureSettings settings, PipelineRuntimeResources resources, in VirtualTexturePhysicalAtlasOverrides physicalAtlasOverrides, GPUDrivenBatchRendererGroup brg)
	{
		m_BRG = brg;
		m_BRG.OnCreatedMaterials += OnBRGCreatedMaterials;
		m_Settings = settings;
		m_Resources = resources;
		m_FrameId = -1;
		m_ChangedMaterials = new HashSet<Material>();
		m_DestroyedMaterials = new HashSet<int>();
		m_FeedbackBuffer = new FeedbackBuffer();
		FeedbackConsumptionTracker = new FeedbackConsumptionTracker(settings.FeedbackMipBiasSettings);
		m_IndirectTextureRenderer = new IndirectionTextureRenderer(m_Resources.VTDrawPageTablePS);
		m_PhysicalAtlas = new PhysicalAtlas(m_Settings.GPUAtlasSizeInMegaBytes, physicalAtlasOverrides.MaxSliceResolution);
		m_VirtualAtlas = new VirtualAtlas(m_PhysicalAtlas.Resolution, OnVirtualAtlasResolutionChanged, delegate
		{
			UpdateMaterials(MaterialUpdateType.OnChange);
		});
		m_AsyncContext = new AsyncContext(m_VirtualAtlas, m_PhysicalAtlas, m_IndirectTextureRenderer, FeedbackConsumptionTracker);
		m_PostRenderWorker = new PostRenderWorker(m_Settings, m_Resources.VTCopyTileCS);
		m_MaterialMetadataRepository = new MaterialMetadataRepository();
		m_MaterialTracker = new MaterialTracker(this, ObjectDispatcherService.TypeTrackingFlags.Default);
		ObjectDispatcherService.RegisterObjectTracker(m_MaterialTracker);
		m_TmpPropertyList = new NativeList<GPUDrivenRenderer.PropertyData>(Allocator.Persistent);
		OnVirtualAtlasResolutionChanged(VirtualAtlasResolutionInTiles);
	}

	public void Dispose()
	{
		m_FeedbackBuffer?.Dispose();
		m_PhysicalAtlas?.Dispose();
		m_IndirectTextureRenderer?.Dispose();
		m_PostRenderWorker.Dispose();
		m_AsyncContext.Dispose();
		m_VirtualAtlas?.Dispose();
		m_MaterialMetadataRepository?.Dispose();
		if (m_TmpPropertyList.IsCreated)
		{
			m_TmpPropertyList.Dispose();
		}
		ObjectDispatcherService.UnregisterObjectTracker(m_MaterialTracker);
		m_BRG.OnCreatedMaterials -= OnBRGCreatedMaterials;
	}

	public void PreRender(CommandBuffer cmd, List<Camera> cameras)
	{
		if (!m_Settings.Enabled)
		{
			return;
		}
		using (new ProfilingScope(ProfilingSampler.Get(ProfileId.VTPreRender)))
		{
			m_FrameId = Time.frameCount;
			if (m_AsyncContext.FrameId != m_FrameId)
			{
				m_AsyncContext.FrameId = m_FrameId;
				cmd.BeginSample("VT PreRender");
				if (!m_VirtualAtlas.IsEmpty)
				{
					ClearFeedbackBuffer(cmd);
				}
				cmd.EndSample("VT PreRender");
			}
		}
	}

	private void OnVirtualAtlasResolutionChanged(int2 virtualAtlasResolutionInTiles)
	{
		m_FeedbackBuffer.Refresh(virtualAtlasResolutionInTiles);
		m_IndirectTextureRenderer.Refresh(virtualAtlasResolutionInTiles);
		m_PostRenderWorker.OnVirtualAtlasResize(in virtualAtlasResolutionInTiles);
	}

	private void OnBRGCreatedMaterials(NativeArray<int>.ReadOnly materialIDs)
	{
		MaterialUpdateContext context = CreateMaterialUpdateContext(MaterialUpdateType.OnChange);
		foreach (int item2 in materialIDs)
		{
			if (m_VirtualAtlas.MaterialStackIndices.TryGetValue(item2, out var item))
			{
				UpdateMaterial(in context, item2, in item);
			}
		}
	}

	private void UpdateMaterials(MaterialUpdateType updateType)
	{
		using (new ProfilingScope(ProfilingSampler.Get(ProfileId.VTUpdateMaterials)))
		{
			NativeKeyValueArrays<int, MaterialStackIndices> keyValueArrays = m_VirtualAtlas.MaterialStackIndices.GetKeyValueArrays(Allocator.Temp);
			List<UnityEngine.Object> value;
			using (ListPool<UnityEngine.Object>.Get(out value))
			{
				Resources.InstanceIDToObjectList(keyValueArrays.Keys, value);
				MaterialUpdateContext context = CreateMaterialUpdateContext(updateType);
				for (int i = 0; i < keyValueArrays.Keys.Length; i++)
				{
					int materialInstanceID = keyValueArrays.Keys[i];
					Material material = value[i] as Material;
					UpdateMaterial(in context, materialInstanceID, material, in UnsafeCollectionExtensions.ElementAsRef(in keyValueArrays.Values, i));
				}
			}
		}
	}

	private void UpdateMaterial(in MaterialUpdateContext context, int materialInstanceID, in MaterialStackIndices stackIdIndices)
	{
		Material material = ObjectDispatcherService.FindMaterialByInstanceId(materialInstanceID);
		UpdateMaterial(in context, materialInstanceID, material, in stackIdIndices);
	}

	private void UpdateMaterial(in MaterialUpdateContext context, int materialInstanceID, Material material, in MaterialStackIndices stackIdIndices)
	{
		if (material == null)
		{
			if (context.WriteToBRG)
			{
				WriteDefaultBRGMaterialData(materialInstanceID);
			}
			return;
		}
		m_StackIdMtx = Matrix4x4.zero;
		for (int i = 0; i < stackIdIndices.Count; i++)
		{
			m_StackIdMtx[i] = stackIdIndices[i];
		}
		SetMatrixIfChanged(material, ShaderPropertyId._VTStackIndices, m_StackIdMtx);
		if (context.WriteToBRG)
		{
			WriteBRGMaterialData(material);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void SetMatrixIfChanged(Material material, int nameID, Matrix4x4 matrix)
		{
			if (!material.HasMatrix(nameID) || material.GetMatrix(nameID) != matrix)
			{
				material.SetMatrix(nameID, matrix);
			}
		}
	}

	private void WriteBRGMaterialData(Material material)
	{
		if (m_BRG.TryFindMaterialIndexAllocation(material, out var indexAllocation))
		{
			m_TmpPropertyList.Clear();
			ref NativeList<GPUDrivenRenderer.PropertyData> tmpPropertyList = ref m_TmpPropertyList;
			GPUDrivenRenderer.PropertyData value = PerMaterialVTStackIdPropertyData(in m_StackIdMtx);
			tmpPropertyList.Add(in value);
			m_BRG.WriteMaterialData(indexAllocation, m_TmpPropertyList.AsArray());
		}
	}

	private void WriteDefaultBRGMaterialData(int materialInstanceID)
	{
		if (m_BRG.TryFindMaterialIndexAllocation(materialInstanceID, out var indexAllocation))
		{
			m_TmpPropertyList.Clear();
			ref NativeList<GPUDrivenRenderer.PropertyData> tmpPropertyList = ref m_TmpPropertyList;
			Matrix4x4 textureStackIndices = Matrix4x4.zero;
			GPUDrivenRenderer.PropertyData value = PerMaterialVTStackIdPropertyData(in textureStackIndices);
			tmpPropertyList.Add(in value);
			m_BRG.WriteDefaultMaterialData(indexAllocation);
		}
	}

	private static GPUDrivenRenderer.PropertyData PerMaterialVTStackIdPropertyData(in Matrix4x4 textureStackIndices)
	{
		return GPUDrivenRenderer.PropertyData.Matrix(ShaderPropertyId._VTStackIndices, textureStackIndices);
	}

	private void ClearFeedbackBuffer(CommandBuffer cmd)
	{
		cmd.SetComputeBufferParam(m_Resources.VTFeedbackCS, 0, ShaderPropertyId._VTFeedbackBuffer, m_FeedbackBuffer.PackedFeedbackBufferUAV);
		cmd.SetComputeIntParam(m_Resources.VTFeedbackCS, ShaderPropertyId._VTFeedbackBufferLength, m_FeedbackBuffer.PackedFeedbackBufferUAV.count);
		cmd.DispatchCompute(m_Resources.VTFeedbackCS, 0, RenderingUtils.DivRoundUp(m_FeedbackBuffer.PackedFeedbackBufferUAV.count, 64), 1, 1);
		cmd.SetRenderTarget(m_FeedbackBuffer.FeedbackRT);
		cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear);
	}

	public void PostRender(CommandBuffer cmd, List<Camera> cameras)
	{
		using (new ProfilingScope(ProfilingSampler.Get(ProfileId.VTPostRender)))
		{
			if (CanTick(cameras))
			{
				AsyncTick(cmd);
			}
		}
	}

	private bool CanTick(List<Camera> cameras)
	{
		if (!m_Settings.Enabled)
		{
			return false;
		}
		if (cameras.Count == 0)
		{
			return false;
		}
		if (cameras.Count == 0 || cameras[0].cameraType != CameraType.Game)
		{
			return false;
		}
		if (m_VirtualAtlas.IsEmpty)
		{
			return false;
		}
		return true;
	}

	private void AsyncTick(CommandBuffer cmd)
	{
		bool loadFeedback = true;
		if (m_AsyncContext.ReadbackProcessor.HasAnyFreeRequests())
		{
			PackFeedbackBuffer(cmd);
			m_AsyncContext.ReadbackProcessor.RequestReadback(cmd, m_FeedbackBuffer.PackedFeedbackBufferUAV);
		}
		if (!m_PostRenderWorker.IsBusy)
		{
			m_PostRenderWorker.LoadResidentTiles(m_AsyncContext, cmd);
			DoMainThreadWork();
			m_PostRenderWorker.UploadBatches(m_AsyncContext, cmd);
			m_AsyncContext.IndirectTextureRenderer.Render(cmd);
			m_AsyncContext.BeforeRunAsync();
			m_PostRenderWorker.RunAsync(m_AsyncContext, cmd, loadFeedback);
		}
	}

	private void DoMainThreadWork()
	{
		UpdateAsyncStats();
		RemoveDestroyedMaterialsFromAtlas();
		ProcessChangedMaterialsInAtlas();
	}

	private void RemoveDestroyedMaterialsFromAtlas()
	{
		using (new ProfilingScope(ProfilingSampler.Get(ProfileId.VTRemoveDestroyedMaterialsFromAtlas)))
		{
			if (m_DestroyedMaterials.Count > 0)
			{
				m_VirtualAtlas.RemoveMaterials(m_DestroyedMaterials);
				m_DestroyedMaterials.Clear();
			}
		}
	}

	private void ProcessChangedMaterialsInAtlas()
	{
		using (new ProfilingScope(ProfilingSampler.Get(ProfileId.VTUpdateChangedMaterialsInAtlas)))
		{
			if (m_ChangedMaterials.Count > 0)
			{
				m_VirtualAtlas.UpdateMaterials(m_ChangedMaterials);
				m_ChangedMaterials.Clear();
			}
		}
	}

	private void PackFeedbackBuffer(CommandBuffer cmd)
	{
		cmd.SetComputeBufferParam(m_Resources.VTFeedbackCS, 1, ShaderPropertyId._VTFeedbackBuffer, m_FeedbackBuffer.PackedFeedbackBufferUAV);
		cmd.SetComputeTextureParam(m_Resources.VTFeedbackCS, 1, ShaderPropertyId._VTFeedbackRT, m_FeedbackBuffer.FeedbackRT);
		cmd.SetComputeIntParam(m_Resources.VTFeedbackCS, ShaderPropertyId._VTFeedbackBufferLength, m_FeedbackBuffer.PackedFeedbackBufferUAV.count);
		cmd.SetComputeIntParam(m_Resources.VTFeedbackCS, ShaderPropertyId._VirtualAtlasWidthInTiles, VirtualAtlasResolutionInTiles.x);
		cmd.DispatchCompute(m_Resources.VTFeedbackCS, 1, RenderingUtils.DivRoundUp(m_FeedbackBuffer.PackedFeedbackBufferUAV.count, 64), 1, 1);
	}

	private void UpdateAsyncStats()
	{
		Counters.VirtualAtlasOccupancy.Value = m_VirtualAtlas.Occupancy;
		Counters.TilesLoadedPerFrame.Value = m_PostRenderWorker.TilesLoadedPerFrame;
		Counters.TilesLoadingLag.Value = m_PostRenderWorker.TilesLoadingLag;
		Counters.TilesLoadedTotal.Value = m_PostRenderWorker.TilesLoadedTotal;
		Counters.FeedbackConsumption.Value = FeedbackConsumptionTracker.FindMaxOfConsumptionMemory();
	}

	internal void PushGlobals(CommandBuffer cmd, in float2 globalMipBias)
	{
		if (m_VirtualAtlas != null && math.all(VirtualAtlasResolutionInTiles > 0))
		{
			cmd.SetGlobalTexture(ShaderPropertyId._VTAtlas, m_PhysicalAtlas.AtlasTex);
			cmd.SetGlobalTexture(ShaderPropertyId._VTIndirectTex, m_IndirectTextureRenderer.IndirectTexture);
			ConfigureCB(in globalMipBias);
			ConstantBuffer.PushGlobal(cmd, in m_AsyncContext.ConstantBuffer, ShaderPropertyId.VirtualTextureConstantBuffer);
			cmd.SetGlobalBuffer(ShaderPropertyId._VTTextureStackDataBuffer, m_VirtualAtlas.TextureStackDataBuffer);
		}
	}

	private void ConfigureCB(in float2 globalMipBias)
	{
		m_AsyncContext.ConstantBuffer._VTPageParams = new float4(m_AsyncContext.VirtualAtlas.ResolutionInTiles.x, m_AsyncContext.VirtualAtlas.ResolutionInTiles.y, m_AsyncContext.VirtualAtlas.MipCount - 1, 0f);
		float2 x = m_AsyncContext.PhysicalAtlas.Resolution.TilesInSlice;
		float2 @float = math.rcp(x);
		m_AsyncContext.ConstantBuffer._VTTileParams = new float4(1f / 18f, 8f / 9f, 144f / (float)m_AsyncContext.PhysicalAtlas.Resolution.PixelsInSlice.x, 144f / (float)m_AsyncContext.PhysicalAtlas.Resolution.PixelsInSlice.y);
		m_AsyncContext.ConstantBuffer._VTPhysicalAtlasSize = new float4(x.x, x.y, @float.x, @float.y);
		float num = math.pow(2f, m_AsyncContext.FeedbackConsumptionTracker.MipBias);
		m_AsyncContext.ConstantBuffer._VTFeedbackMipBias = num * globalMipBias.y;
	}

	public void ProcessChangedMaterials(List<Material> materials)
	{
		for (int i = 0; i < materials.Count; i++)
		{
			m_ChangedMaterials.Add(materials[i]);
		}
	}

	internal void ProcessDestroyedMaterials(List<int> materialsToRemove)
	{
		for (int i = 0; i < materialsToRemove.Count; i++)
		{
			m_DestroyedMaterials.Add(materialsToRemove[i]);
		}
	}

	internal bool HasMaterial(int materialId)
	{
		return m_VirtualAtlas.HasMaterial(materialId);
	}

	public void Reset()
	{
		if (m_VirtualAtlas != null)
		{
			m_VirtualAtlas.Reset();
			OnVirtualAtlasResolutionChanged(VirtualAtlasResolutionInTiles);
			UpdateMaterials(MaterialUpdateType.OnChange);
		}
	}

	public static void SetFeedbackBufferRandomWriteTarget(CommandBuffer cmd, RenderTexture vtFeedbackRt)
	{
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal)
		{
			cmd.SetRandomWriteTarget(7, vtFeedbackRt);
		}
	}

	private MaterialUpdateContext CreateMaterialUpdateContext(MaterialUpdateType type)
	{
		MaterialUpdateContext result = default(MaterialUpdateContext);
		result.WriteToBRG = type != MaterialUpdateType.Forced && m_BRG.IsEnabledAndInitialized;
		return result;
	}

	internal void CalculateAndLogTextureMemory()
	{
		Debug.LogWarning("This function is available only in editor");
	}
}
