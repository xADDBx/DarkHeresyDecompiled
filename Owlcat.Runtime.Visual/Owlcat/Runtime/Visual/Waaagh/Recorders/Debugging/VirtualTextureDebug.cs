using System;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public static class VirtualTextureDebug
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_VTFeedback = Shader.PropertyToID("_Debug_VTFeedback");

		public static readonly int _Debug_AtlasScaleOffset = Shader.PropertyToID("_Debug_AtlasScaleOffset");

		public static readonly int _VirtualAtlasSize = Shader.PropertyToID("_VirtualAtlasSize");

		public static readonly int _Debug_VTPhysicalAtlas = Shader.PropertyToID("_Debug_VTPhysicalAtlas");

		public static readonly int _Debug_VTPhysicalAtlasArraySlices = Shader.PropertyToID("_Debug_VTPhysicalAtlasArraySlices");

		public static readonly int _Debug_ShowSliceGrid = Shader.PropertyToID("_Debug_ShowSliceGrid");

		public static readonly int _Debug_VTBatchedCopyRt = Shader.PropertyToID("_Debug_VTBatchedCopyRt");

		public static readonly int m_Debug_MipLevel = Shader.PropertyToID("m_Debug_MipLevel");

		public static readonly int _Debug_VTIndirectTexture = Shader.PropertyToID("_Debug_VTIndirectTexture");

		public static readonly int _VTDebugVirtualAtlasBuffer = Shader.PropertyToID("_VTDebugVirtualAtlasBuffer");

		public static readonly int _Color = Shader.PropertyToID("_Color");

		public static readonly int _Offset = Shader.PropertyToID("_Offset");

		public static readonly int _VTAtlasSize = Shader.PropertyToID("_VTAtlasSize");
	}

	public class VirtualTextureFeedbackDebugPassData
	{
		public Material Material;

		public VirtualTextureManager VirtualTextureManager;

		public bool ShowFeedback;

		public TextureHandle CameraFinalTarget;

		public TextureHandle FeedbackDebugTexture;

		public TextureHandle FeedbackRT;

		public float2 ScreenSize;

		public float2 FeedbackDebugTextureSize;

		public float Scale;

		public int ShowFeedbackPass;

		public int DecodeFeedbackPass;

		public bool FlipVertically;
	}

	public class VirtualTextureShowPhysicalAtlasPassData
	{
		public Material Material;

		public int Pass;

		public VirtualTextureManager VirtualTextureManager;

		public TextureHandle CameraFinalTarget;

		public bool ShowPhysicalAtlas;

		public bool ShowSliceGrid;

		public float Scale;

		public Texture2DArray PhysicalAtlasTex;

		public float2 ScreenSize;

		public bool FlipVertically;
	}

	public class VirtualTextureShowBatchedCopyRtPassData
	{
		public Material Material;

		public int Pass;

		public VirtualTextureManager VirtualTextureManager;

		public TextureHandle CameraFinalTarget;

		public bool ShowBatchedCopyRt;

		public float Scale;

		public float2 ScreenSize;

		public RenderTexture BatchedCopyRt;

		public Texture2D BatchedCopyDebugTexture;

		public int Limit;

		public bool FlipVertically;
	}

	public class VirtualTextureShowIndirectTexturePassData
	{
		public Material Material;

		public int Pass;

		public VirtualTextureManager VirtualTextureManager;

		public TextureHandle CameraFinalTarget;

		public bool ShowIndirectTexture;

		public RTHandle IndirectTexture;

		public float2 ScreenSize;

		public float Scale;

		public bool FlipVertically;
	}

	public class VirtualTextureShowVirtualAtlasPassData
	{
		public Material Material;

		public int Pass;

		public float Scale;

		public float2 ScreenSize;

		public TextureHandle CameraColor;

		public BufferHandle RectanglesBuffer;

		public VirtualTextureManager VirtualTextureManager;

		public NativeList<int4> Rects;

		public int UsedRectsCount;

		public int FreeRectsCount;

		public bool FlipVertically;
	}

	public static void Record(in RecordContext context)
	{
		if (context.IsVTEnabled && !context.VirtualTextureManager.IsVirtualAtlasEmpty)
		{
			if (context.DebugContext.DebugData.VirtualTextureDebug.ShowFeedback)
			{
				DrawFeedback(in context);
			}
			if (context.DebugContext.DebugData.VirtualTextureDebug.ShowPhysicalAtlas)
			{
				DrawPhysicalAtlas(in context);
			}
			if (context.DebugContext.DebugData.VirtualTextureDebug.ShowBatchedCopyRt)
			{
				DrawBatchedCopyRt(context);
			}
			if (context.DebugContext.DebugData.VirtualTextureDebug.ShowIndirectionTexture)
			{
				DrawIndirectionTexture(in context);
			}
			if (context.DebugContext.DebugData.VirtualTextureDebug.ShowVirtualAtlas)
			{
				DrawVirtualAtlas(in context);
			}
		}
	}

	private static void DrawFeedback(in RecordContext context)
	{
		VirtualTextureFeedbackDebugPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<VirtualTextureFeedbackDebugPassData>("DEBUG - VT Feedback", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\VirtualTextureDebug.cs", 86);
		passData.Material = context.DebugContext.MaterialLibrary.VirtualTextureDebug;
		passData.ShowFeedbackPass = context.DebugContext.MaterialLibrary.ShowFeedbackTexturePass;
		passData.DecodeFeedbackPass = context.DebugContext.MaterialLibrary.DecodeFeedbackPass;
		passData.VirtualTextureManager = context.VirtualTextureManager;
		passData.ShowFeedback = context.DebugContext.DebugData.VirtualTextureDebug.ShowFeedback;
		if (passData.ShowFeedback)
		{
			TextureDesc desc = new TextureDesc(passData.VirtualTextureManager.VirtualAtlasResolutionInTiles.x, passData.VirtualTextureManager.VirtualAtlasResolutionInTiles.y);
			desc.useMipMap = false;
			desc.clearBuffer = false;
			desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			desc.name = "VTFeedbackDebug";
			passData.FeedbackDebugTexture = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
			passData.FeedbackRT = context.FrameResources.VTFeedbackData.VTFeedback;
			unsafeRenderGraphBuilder.UseTexture(in passData.FeedbackRT);
			passData.FeedbackDebugTextureSize = new float2(passData.VirtualTextureManager.VirtualAtlasResolutionInTiles.x, passData.VirtualTextureManager.VirtualAtlasResolutionInTiles.y);
		}
		passData.CameraFinalTarget = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraFinalTarget, AccessFlags.Write);
		passData.ScreenSize = new float2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		passData.Scale = context.DebugContext.DebugData.VirtualTextureDebug.FeedbackScale;
		passData.FlipVertically = false;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VirtualTextureFeedbackDebugPassData data, UnsafeGraphContext context)
		{
			if (data.ShowFeedback)
			{
				CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
				context.cmd.SetGlobalVector(ShaderPropertyId._VirtualAtlasSize, new Vector4(data.FeedbackDebugTextureSize.x, data.FeedbackDebugTextureSize.y, 0f, 0f));
				context.cmd.SetGlobalTexture(Owlcat.Runtime.Visual.VirtualTexture.ShaderPropertyId._VTFeedbackRT, data.FeedbackRT);
				nativeCommandBuffer.Blit(context.defaultResources.whiteTexture, data.FeedbackDebugTexture, data.Material, data.DecodeFeedbackPass);
				float2 screenSize = data.ScreenSize;
				float2 @float = data.FeedbackDebugTextureSize * data.Scale / screenSize;
				float2 float2 = new float2(1f - @float.x, 0f - (1f - @float.y));
				Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTFeedback, data.FeedbackDebugTexture);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
				nativeCommandBuffer.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.ShowFeedbackPass);
			}
		});
	}

	private static void DrawPhysicalAtlas(in RecordContext context)
	{
		VirtualTextureShowPhysicalAtlasPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<VirtualTextureShowPhysicalAtlasPassData>("DEBUG - VT Physical Atlas", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\VirtualTextureDebug.cs", 155);
		passData.Material = context.DebugContext.MaterialLibrary.VirtualTextureDebug;
		passData.Pass = context.DebugContext.MaterialLibrary.PhysicalAtlasDebugPass;
		passData.VirtualTextureManager = context.VirtualTextureManager;
		passData.CameraFinalTarget = context.FrameResources.CameraStackTargets.Color;
		TextureHandle input = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in input, AccessFlags.Write);
		passData.ShowPhysicalAtlas = context.DebugContext.DebugData.VirtualTextureDebug.ShowPhysicalAtlas;
		passData.ShowSliceGrid = context.DebugContext.DebugData.VirtualTextureDebug.ShowPhysicalAtlasSliceGrid;
		passData.Scale = context.DebugContext.DebugData.VirtualTextureDebug.PhyscalAtlasScale;
		passData.PhysicalAtlasTex = context.VirtualTextureManager.CacheTex;
		passData.ScreenSize = new float2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		passData.FlipVertically = false;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VirtualTextureShowPhysicalAtlasPassData data, UnsafeGraphContext context)
		{
			if (data.ShowPhysicalAtlas)
			{
				float2 screenSize = data.ScreenSize;
				float2 @float = (float2)(math.min(screenSize.x, screenSize.y) * data.Scale) / screenSize;
				float2 float2 = new float2(0f - (1f - @float.x), 0f - (1f - @float.y));
				Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTPhysicalAtlas, data.PhysicalAtlasTex);
				context.cmd.SetGlobalInteger(ShaderPropertyId._Debug_VTPhysicalAtlasArraySlices, data.PhysicalAtlasTex.depth);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
				context.cmd.SetGlobalInteger(ShaderPropertyId._Debug_ShowSliceGrid, data.ShowSliceGrid ? 1 : 0);
				CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.Pass);
			}
		});
	}

	private static void DrawBatchedCopyRt(RecordContext context)
	{
		VirtualTextureShowBatchedCopyRtPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<VirtualTextureShowBatchedCopyRtPassData>("DEBUG - VT Batched Copy Rt", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\VirtualTextureDebug.cs", 207);
		passData.Material = context.DebugContext.MaterialLibrary.VirtualTextureDebug;
		passData.Pass = context.DebugContext.MaterialLibrary.ShowBatchedCopyRtPass;
		passData.VirtualTextureManager = context.VirtualTextureManager;
		passData.CameraFinalTarget = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraFinalTarget, AccessFlags.Write);
		passData.ShowBatchedCopyRt = context.DebugContext.DebugData.VirtualTextureDebug.ShowBatchedCopyRt;
		if (passData.ShowBatchedCopyRt)
		{
			passData.BatchedCopyRt = context.VirtualTextureManager.TileUploader.BatchedCopyRt;
			passData.BatchedCopyDebugTexture = context.VirtualTextureManager.TileUploader.BatchedCopyDebugTex;
			passData.Scale = context.DebugContext.DebugData.VirtualTextureDebug.BatchedCopyRtScale;
			passData.ScreenSize = new float2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
			passData.Limit = (int)context.VirtualTextureManager.TileUploader.Limit;
		}
		passData.FlipVertically = false;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VirtualTextureShowBatchedCopyRtPassData data, UnsafeGraphContext context)
		{
			if (data.ShowBatchedCopyRt)
			{
				context.cmd.CopyTexture(data.BatchedCopyRt, 0, 0, 0, 0, data.BatchedCopyRt.width, data.BatchedCopyRt.height, data.BatchedCopyDebugTexture, 0, 0, 0, 0);
				float2 screenSize = data.ScreenSize;
				float2 @float = (float2)(math.min(screenSize.x, screenSize.y) * data.Scale) / screenSize;
				float2 float2 = new float2(0f - (1f - @float.x), 1f - @float.y);
				Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTBatchedCopyRt, data.BatchedCopyDebugTexture);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
				context.cmd.SetGlobalFloat(ShaderPropertyId.m_Debug_MipLevel, 0f);
				CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.Pass);
			}
		});
	}

	private static void DrawIndirectionTexture(in RecordContext context)
	{
		VirtualTextureShowIndirectTexturePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<VirtualTextureShowIndirectTexturePassData>("DEBUG - VT Indirection Texture", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\VirtualTextureDebug.cs", 276);
		passData.Material = context.DebugContext.MaterialLibrary.VirtualTextureDebug;
		passData.Pass = context.DebugContext.MaterialLibrary.IndirectTextureDebugPass;
		passData.VirtualTextureManager = context.VirtualTextureManager;
		passData.CameraFinalTarget = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraFinalTarget, AccessFlags.Write);
		passData.ShowIndirectTexture = context.DebugContext.DebugData.VirtualTextureDebug.ShowIndirectionTexture;
		passData.IndirectTexture = context.VirtualTextureManager.IndirectTexture;
		passData.ScreenSize = new float2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		passData.Scale = context.DebugContext.DebugData.VirtualTextureDebug.IndirectTextureScale;
		passData.FlipVertically = false;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VirtualTextureShowIndirectTexturePassData data, UnsafeGraphContext context)
		{
			if (data.ShowIndirectTexture)
			{
				float2 screenSize = data.ScreenSize;
				float2 @float = new float2(data.IndirectTexture.rt.width, data.IndirectTexture.rt.height) * data.Scale / screenSize;
				float2 float2 = 1f - @float;
				Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTIndirectTexture, data.IndirectTexture);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
				CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.Pass);
			}
		});
	}

	private static void DrawVirtualAtlas(in RecordContext context)
	{
		VirtualTextureShowVirtualAtlasPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<VirtualTextureShowVirtualAtlasPassData>("DEBUG - VT Virtual Atlas", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\VirtualTextureDebug.cs", 325);
		passData.Material = context.DebugContext.MaterialLibrary.VirtualTextureDebug;
		passData.Pass = context.DebugContext.MaterialLibrary.VirtualAtlasDebugPass;
		passData.VirtualTextureManager = context.VirtualTextureManager;
		passData.Rects = new NativeList<int4>(16, Allocator.Temp);
		_ = context.VirtualTextureManager.AtlasAllocator;
		Span<Node> span = context.VirtualTextureManager.AtlasAllocator.Nodes.AsArray().AsSpan();
		passData.UsedRectsCount = 0;
		for (int i = 0; i < span.Length; i++)
		{
			ref Node reference = ref span[i];
			if (reference.Kind == NodeKind.Alloc)
			{
				ref NativeList<int4> rects = ref passData.Rects;
				int4 value = reference.Rect.Value;
				rects.Add(in value);
				passData.UsedRectsCount++;
			}
		}
		passData.FreeRectsCount = 0;
		for (int j = 0; j < span.Length; j++)
		{
			ref Node reference2 = ref span[j];
			if (reference2.Kind == NodeKind.Free)
			{
				ref NativeList<int4> rects2 = ref passData.Rects;
				int4 value = reference2.Rect.Value;
				rects2.Add(in value);
				passData.FreeRectsCount++;
			}
		}
		BufferDesc bufferDesc = default(BufferDesc);
		bufferDesc.count = passData.Rects.Length;
		bufferDesc.stride = Marshal.SizeOf<int4>();
		bufferDesc.name = "VTDebugVirtualAtlasBuffer";
		bufferDesc.target = GraphicsBuffer.Target.Structured;
		BufferDesc desc = bufferDesc;
		passData.CameraColor = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraColor, AccessFlags.Write);
		passData.RectanglesBuffer = unsafeRenderGraphBuilder.CreateTransientBuffer(in desc);
		passData.Scale = context.DebugContext.DebugData.VirtualTextureDebug.VirtualAtlasScale;
		passData.ScreenSize = new float2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		passData.FlipVertically = false;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VirtualTextureShowVirtualAtlasPassData data, UnsafeGraphContext context)
		{
			AtlasAllocator atlasAllocator = data.VirtualTextureManager.AtlasAllocator;
			context.cmd.SetBufferData(data.RectanglesBuffer, data.Rects.AsArray());
			float2 screenSize = data.ScreenSize;
			float2 @float = new float2(atlasAllocator.Width, atlasAllocator.Height) * data.Scale / screenSize;
			float2 float2 = new float2(0f - (1f - @float.x) - @float.x, 1f - @float.y);
			Vector4 value2 = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
			context.cmd.SetRenderTarget(data.CameraColor);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value2);
			context.cmd.SetGlobalBuffer(ShaderPropertyId._VTDebugVirtualAtlasBuffer, data.RectanglesBuffer);
			context.cmd.SetGlobalColor(ShaderPropertyId._Color, Color.red);
			context.cmd.SetGlobalFloat(ShaderPropertyId._Offset, 0f);
			context.cmd.SetGlobalVector(ShaderPropertyId._VTAtlasSize, new Vector4(math.rcp(data.VirtualTextureManager.AtlasAllocator.Width), math.rcp(data.VirtualTextureManager.AtlasAllocator.Height) * (float)((!data.FlipVertically) ? 1 : (-1)), 0f, 0f));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Quads, 4, data.UsedRectsCount);
			context.cmd.SetGlobalFloat(ShaderPropertyId._Offset, data.UsedRectsCount);
			context.cmd.SetGlobalColor(ShaderPropertyId._Color, Color.green);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Quads, 4, data.FreeRectsCount);
			data.Rects.Dispose();
		});
	}
}
