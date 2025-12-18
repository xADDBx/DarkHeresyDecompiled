using System;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture.Jobs;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;

public class IndirectionTextureRenderer : IDisposable
{
	private static class ShaderPropertyId
	{
		public static readonly int _PageTableBuffer = Shader.PropertyToID("_PageTableBuffer");
	}

	private RTHandle m_IndirectTexture;

	private NativeList<PageDrawData> m_DrawData;

	private NativeList<PageDrawDataGPU> m_DrawDataGPU;

	private Material m_Material;

	private GraphicsBuffer m_DrawDataBuffer;

	public RTHandle IndirectTexture => m_IndirectTexture;

	public IndirectionTextureRenderer(Shader drawPageTablePS)
	{
		m_DrawData = new NativeList<PageDrawData>(256, Allocator.Persistent);
		m_DrawDataGPU = new NativeList<PageDrawDataGPU>(256, Allocator.Persistent);
		m_Material = CoreUtils.CreateEngineMaterial(drawPageTablePS);
	}

	internal void Refresh(int2 virtualAtlasResolutionInTiles)
	{
		if (m_IndirectTexture == null || virtualAtlasResolutionInTiles.x != m_IndirectTexture.rt.width || virtualAtlasResolutionInTiles.y != m_IndirectTexture.rt.height)
		{
			RTHandle rTHandle = RTHandles.Alloc(math.max(1, virtualAtlasResolutionInTiles.x), math.max(1, virtualAtlasResolutionInTiles.y), 1, DepthBits.None, GraphicsFormat.R8G8B8A8_UNorm, FilterMode.Point, TextureWrapMode.Clamp, TextureDimension.Tex2D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: false, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, "VTIndirectTex");
			if (m_IndirectTexture != null)
			{
				Graphics.CopyTexture(m_IndirectTexture.rt, 0, 0, 0, 0, m_IndirectTexture.rt.width, m_IndirectTexture.rt.height, rTHandle.rt, 0, 0, 0, 0);
			}
			DisposeVirtualAtlasDependentData();
			m_IndirectTexture = rTHandle;
			int num = math.max(1, virtualAtlasResolutionInTiles.x * virtualAtlasResolutionInTiles.y);
			m_DrawDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, Marshal.SizeOf<PageDrawDataGPU>());
			m_DrawData = new NativeList<PageDrawData>(num, Allocator.Persistent);
			m_DrawDataGPU = new NativeList<PageDrawDataGPU>(num, Allocator.Persistent);
		}
	}

	internal JobHandle PrepareData(AsyncContext context, JobHandle dependency)
	{
		m_DrawData.Clear();
		m_DrawDataGPU.Clear();
		PageDrawDataBuildJob pageDrawDataBuildJob = default(PageDrawDataBuildJob);
		pageDrawDataBuildJob.FrameId = context.FrameId;
		pageDrawDataBuildJob.VirtualAtlasResolutionInTiles = context.VirtualAtlas.ResolutionInTiles;
		pageDrawDataBuildJob.InvPhysicalAtlasSliceResolutionInTiles = math.rcp(context.PhysicalAtlas.Resolution.TilesInSlice);
		pageDrawDataBuildJob.PageEnumerator = context.VirtualAtlas.PhysicalToVirtualPageMap.GetEnumerator();
		pageDrawDataBuildJob.Pages = context.VirtualAtlas.Pages;
		pageDrawDataBuildJob.Entries = context.VirtualAtlas.Entries;
		pageDrawDataBuildJob.DrawData = m_DrawData;
		PageDrawDataBuildJob jobData = pageDrawDataBuildJob;
		dependency = IJobExtensions.ScheduleByRef(ref jobData, dependency);
		SortListJob<PageDrawData> sortListJob = default(SortListJob<PageDrawData>);
		sortListJob.List = m_DrawData;
		SortListJob<PageDrawData> jobData2 = sortListJob;
		dependency = IJobExtensions.ScheduleByRef(ref jobData2, dependency);
		PageDrawDataGPUBuildJob pageDrawDataGPUBuildJob = default(PageDrawDataGPUBuildJob);
		pageDrawDataGPUBuildJob.VirtualAtlasResolutionInTiles = context.VirtualAtlas.ResolutionInTiles;
		pageDrawDataGPUBuildJob.DrawData = m_DrawData;
		pageDrawDataGPUBuildJob.DrawDataGPU = m_DrawDataGPU;
		PageDrawDataGPUBuildJob jobData3 = pageDrawDataGPUBuildJob;
		dependency = IJobExtensions.ScheduleByRef(ref jobData3, dependency);
		return dependency;
	}

	internal void Render(CommandBuffer cmd)
	{
		int length = m_DrawData.Length;
		if (length > 0)
		{
			cmd.SetBufferData(m_DrawDataBuffer, m_DrawDataGPU.AsArray(), 0, 0, length);
			cmd.SetGlobalBuffer(ShaderPropertyId._PageTableBuffer, m_DrawDataBuffer);
			cmd.SetRenderTarget(m_IndirectTexture);
			cmd.DrawProcedural(Matrix4x4.identity, m_Material, 0, MeshTopology.Quads, 4, length);
			cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
		}
	}

	public void Dispose()
	{
		DisposeVirtualAtlasDependentData();
		CoreUtils.Destroy(m_Material);
	}

	private void DisposeVirtualAtlasDependentData()
	{
		if (m_IndirectTexture != null)
		{
			RTHandles.Release(m_IndirectTexture);
			m_IndirectTexture = null;
		}
		m_DrawDataBuffer?.Dispose();
		if (m_DrawData.IsCreated)
		{
			m_DrawData.Dispose();
		}
		if (m_DrawDataGPU.IsCreated)
		{
			m_DrawDataGPU.Dispose();
		}
	}
}
