using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.Streaming;

public class TileUploader : IDisposable
{
	private TilesInBatchLimit m_Limit;

	private int2 m_LimitSize;

	private ComputeShader m_CopyTilesCS;

	private int m_BatchedCopyKernerIndex;

	private int2 m_BatchedCopyKernelSize;

	private RTHandle m_BatchedTilesRt;

	private GraphicsBuffer m_UploadBuffer;

	private int m_TilesLoadedPerFrame;

	private int m_TilesLoadedTotal;

	private int m_TilesLoadingLag;

	private Texture2D m_BatchedCopyDebugTex;

	public int TilesLoadedPerFrame => m_TilesLoadedPerFrame;

	public int TilesLoadedTotal => m_TilesLoadedTotal;

	public int TilesLoadingLag => m_TilesLoadingLag;

	public RTHandle BatchedCopyRt => m_BatchedTilesRt;

	public TilesInBatchLimit Limit => m_Limit;

	public Texture2D BatchedCopyDebugTex
	{
		get
		{
			RefreshBatchedCopyDebugTex();
			return m_BatchedCopyDebugTex;
		}
	}

	public TileUploader(TilesInBatchLimit limit, ComputeShader copyTilesCS)
	{
		m_Limit = limit;
		m_CopyTilesCS = copyTilesCS;
		m_BatchedCopyKernerIndex = m_CopyTilesCS.FindKernel("BatchedCopyTiles");
		m_CopyTilesCS.GetKernelThreadGroupSizes(m_BatchedCopyKernerIndex, out var x, out var y, out var _);
		m_BatchedCopyKernelSize = new int2((int)x, (int)y);
		switch (m_Limit)
		{
		case TilesInBatchLimit.x16:
			m_LimitSize = new int2(4, 4);
			break;
		case TilesInBatchLimit.x32:
			m_LimitSize = new int2(8, 4);
			break;
		case TilesInBatchLimit.x64:
			m_LimitSize = new int2(8, 8);
			break;
		}
		int2 @int = m_LimitSize * 144 / 4;
		RenderTextureDescriptor descriptor = new RenderTextureDescriptor
		{
			width = @int.x,
			height = @int.y,
			volumeDepth = 1,
			graphicsFormat = GraphicsFormat.R32G32B32A32_SInt,
			autoGenerateMips = false,
			mipCount = 2,
			useMipMap = true,
			dimension = TextureDimension.Tex2D,
			msaaSamples = 1,
			enableRandomWrite = true
		};
		m_BatchedTilesRt = RTHandles.Alloc(in descriptor);
		int num = UnsafeUtility.SizeOf<int>();
		int count = (int)m_Limit * 25920 / num;
		m_UploadBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, num);
	}

	public void Dispose()
	{
		RTHandles.Release(m_BatchedTilesRt);
		m_UploadBuffer?.Dispose();
	}

	public void Upload(AsyncContext context, CommandBuffer cmd)
	{
		UploadTiles(context, cmd);
	}

	private void UploadTiles(AsyncContext context, CommandBuffer cmd)
	{
		m_TilesLoadedPerFrame = 0;
		m_TilesLoadingLag = 0;
		if (context.PendingBatches.Count <= 0)
		{
			return;
		}
		cmd.SetComputeIntParam(m_CopyTilesCS, ShaderPropertyId._LimitX, m_LimitSize.x);
		cmd.SetComputeIntParam(m_CopyTilesCS, ShaderPropertyId._LimitY, m_LimitSize.y);
		foreach (StreamingBatch pendingBatch in context.PendingBatches)
		{
			cmd.SetBufferData(m_UploadBuffer, pendingBatch.RawData);
			cmd.SetComputeBufferParam(m_CopyTilesCS, m_BatchedCopyKernerIndex, ShaderPropertyId._RawData, m_UploadBuffer);
			cmd.SetComputeIntParam(m_CopyTilesCS, ShaderPropertyId._MipWidth, 36);
			cmd.SetComputeTextureParam(m_CopyTilesCS, m_BatchedCopyKernerIndex, ShaderPropertyId._Result, m_BatchedTilesRt, 0);
			cmd.DispatchCompute(m_CopyTilesCS, m_BatchedCopyKernerIndex, RenderingUtils.DivRoundUp(m_BatchedTilesRt.rt.width, m_BatchedCopyKernelSize.x), RenderingUtils.DivRoundUp(m_BatchedTilesRt.rt.height, m_BatchedCopyKernelSize.y), 1);
			cmd.SetComputeIntParam(m_CopyTilesCS, ShaderPropertyId._MipWidth, 18);
			cmd.SetComputeTextureParam(m_CopyTilesCS, m_BatchedCopyKernerIndex, ShaderPropertyId._Result, m_BatchedTilesRt, 1);
			cmd.DispatchCompute(m_CopyTilesCS, m_BatchedCopyKernerIndex, RenderingUtils.DivRoundUp(m_BatchedTilesRt.rt.width / 2, m_BatchedCopyKernelSize.x), RenderingUtils.DivRoundUp(m_BatchedTilesRt.rt.height / 2, m_BatchedCopyKernelSize.y), 1);
			int num = -1;
			Span<TileReadTask> span = UnsafeCollectionExtensions.AsSpan(in pendingBatch.Tasks);
			for (int i = 0; i < pendingBatch.Tasks.Length; i++)
			{
				ref TileReadTask reference = ref span[i];
				NativeArray<Page> nativeArray = context.VirtualAtlas.Pages;
				ref Page reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, reference.VirtualCoord.y * context.VirtualAtlas.ResolutionInTiles.x + reference.VirtualCoord.x);
				reference2.IsLoading = false;
				if (reference.LayerIndex == 0)
				{
					num = context.LruCache.GetMostOutdated(reference.LayerCount);
					if (!context.LruCache.SetActive(num, reference.LayerCount))
					{
						num = -1;
					}
				}
				if (num >= 0)
				{
					int3 id = LruLayerCacheUtility.CacheIdToPhysicalTileCoord(num + reference.LayerIndex, in context.PhysicalAtlas.Resolution);
					context.VirtualAtlas.InvalidatePage(in id);
					int2 @int = new int2(i % m_LimitSize.x, i / m_LimitSize.x);
					int2 int2 = @int * 144 / 4;
					cmd.CopyTexture(m_BatchedTilesRt, 0, 0, int2.x, int2.y, 36, 36, context.PhysicalAtlas.AtlasTex, id.z, 0, id.x * 144, id.y * 144);
					int2 int3 = @int * 72 / 4;
					cmd.CopyTexture(m_BatchedTilesRt, 0, 1, int3.x, int3.y, 18, 18, context.PhysicalAtlas.AtlasTex, id.z, 1, id.x * 72, id.y * 72);
					if (reference.LayerIndex == 0)
					{
						reference2.PhysicalTileCoord = id;
						context.VirtualAtlas.PhysicalToVirtualPageMap.Add(id, reference.VirtualCoord);
					}
					m_TilesLoadedPerFrame++;
					m_TilesLoadedTotal++;
					m_TilesLoadingLag = math.max(m_TilesLoadingLag, context.FrameId - reference.FrameId);
				}
			}
		}
	}

	private void RefreshBatchedCopyDebugTex()
	{
		if (m_BatchedCopyDebugTex == null)
		{
			m_BatchedCopyDebugTex = new Texture2D(mipmapLimitDescriptor: new MipmapLimitDescriptor(useMipmapLimit: false, string.Empty), width: m_BatchedTilesRt.rt.width * 4, height: m_BatchedTilesRt.rt.height * 4, textureFormat: TextureFormat.DXT5, mipCount: 1, linear: true, createUninitialized: true);
			m_BatchedCopyDebugTex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		}
	}
}
