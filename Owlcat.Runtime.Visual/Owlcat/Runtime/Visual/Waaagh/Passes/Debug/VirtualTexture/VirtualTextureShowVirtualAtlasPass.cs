using System;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowVirtualAtlasPass : ScriptableRenderPass<VirtualTextureShowVirtualAtlasPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_AtlasScaleOffset = Shader.PropertyToID("_Debug_AtlasScaleOffset");

		public static readonly int _VTDebugVirtualAtlasBuffer = Shader.PropertyToID("_VTDebugVirtualAtlasBuffer");

		public static readonly int _Color = Shader.PropertyToID("_Color");

		public static readonly int _Offset = Shader.PropertyToID("_Offset");

		public static readonly int _VTAtlasSize = Shader.PropertyToID("_VTAtlasSize");
	}

	private WaaaghDebugData m_DebugData;

	private Material m_Material;

	private int m_Pass;

	public override string Name => "VirtualTextureShowVirtualAtlasPass";

	public VirtualTextureShowVirtualAtlasPass(RenderPassEvent evt, WaaaghDebugData debugData, Material material)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Material = material;
		m_Pass = material.FindPass("VirtualAtlasDebug");
	}

	protected override void Setup(RenderGraphBuilder builder, VirtualTextureShowVirtualAtlasPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		data.Material = m_Material;
		data.Pass = m_Pass;
		data.VirtualTextureManager = waaaghRenderingData.VirtualTextureManager;
		data.Rects = new NativeList<int4>(16, Allocator.Temp);
		_ = waaaghRenderingData.VirtualTextureManager.AtlasAllocator;
		Span<Node> span = waaaghRenderingData.VirtualTextureManager.AtlasAllocator.Nodes.AsArray().AsSpan();
		data.UsedRectsCount = 0;
		for (int i = 0; i < span.Length; i++)
		{
			ref Node reference = ref span[i];
			if (reference.Kind == NodeKind.Alloc)
			{
				ref NativeList<int4> rects = ref data.Rects;
				int4 value = reference.Rect.Value;
				rects.Add(in value);
				data.UsedRectsCount++;
			}
		}
		data.FreeRectsCount = 0;
		for (int j = 0; j < span.Length; j++)
		{
			ref Node reference2 = ref span[j];
			if (reference2.Kind == NodeKind.Free)
			{
				ref NativeList<int4> rects2 = ref data.Rects;
				int4 value = reference2.Rect.Value;
				rects2.Add(in value);
				data.FreeRectsCount++;
			}
		}
		BufferDesc bufferDesc = default(BufferDesc);
		bufferDesc.count = data.Rects.Length;
		bufferDesc.stride = Marshal.SizeOf<int4>();
		bufferDesc.name = "VTDebugVirtualAtlasBuffer";
		bufferDesc.target = GraphicsBuffer.Target.Structured;
		BufferDesc desc = bufferDesc;
		data.RectanglesBuffer = builder.CreateTransientBuffer(in desc);
		data.Scale = m_DebugData.VirtualTextureDebug.VirtualAtlasScale;
		data.ScreenSize = new float2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
		data.FlipVertically = waaaghCameraData.resolveToScreen;
	}

	protected override void Render(VirtualTextureShowVirtualAtlasPassData data, RenderGraphContext context)
	{
		AtlasAllocator atlasAllocator = data.VirtualTextureManager.AtlasAllocator;
		context.cmd.SetBufferData(data.RectanglesBuffer, data.Rects.AsArray());
		float2 screenSize = data.ScreenSize;
		float2 @float = new float2(atlasAllocator.Width, atlasAllocator.Height) * data.Scale / screenSize;
		float2 float2 = new float2(0f - (1f - @float.x) - @float.x, 1f - @float.y);
		Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
		context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._VTDebugVirtualAtlasBuffer, data.RectanglesBuffer);
		context.cmd.SetGlobalColor(ShaderPropertyId._Color, Color.red);
		context.cmd.SetGlobalFloat(ShaderPropertyId._Offset, 0f);
		context.cmd.SetGlobalVector(ShaderPropertyId._VTAtlasSize, new Vector4(math.rcp(data.VirtualTextureManager.AtlasAllocator.Width), math.rcp(data.VirtualTextureManager.AtlasAllocator.Height) * (float)((!data.FlipVertically) ? 1 : (-1)), 0f, 0f));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Quads, 4, data.UsedRectsCount);
		context.cmd.SetGlobalFloat(ShaderPropertyId._Offset, data.UsedRectsCount);
		context.cmd.SetGlobalColor(ShaderPropertyId._Color, Color.green);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Quads, 4, data.FreeRectsCount);
		data.Rects.Dispose();
	}
}
