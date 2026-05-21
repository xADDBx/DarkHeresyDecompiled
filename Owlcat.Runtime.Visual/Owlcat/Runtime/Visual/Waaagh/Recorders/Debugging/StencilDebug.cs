using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public static class StencilDebug
{
	private static class ShaderIDs
	{
		public static readonly int _Debug_StecilRef = Shader.PropertyToID("_Debug_StecilRef");

		public static readonly int _Debug_StecilReadMask = Shader.PropertyToID("_Debug_StecilReadMask");

		public static readonly int _Debug_BlitInput = Shader.PropertyToID("_Debug_BlitInput");
	}

	private class PassData
	{
		public TextureHandle CameraColor;

		public TextureHandle CameraDepth;

		public TextureHandle TempRT;

		public Material Material;

		public int StencilDebugPass;

		public int DebugBlitPass;

		public StencilDebugType StencilDebugType;

		public int StencilRef;

		public StencilRef StencilFlags;
	}

	public static void Record(in RecordContext context)
	{
		if (context.DebugContext.DebugData.StencilDebug.StencilDebugType != 0)
		{
			RecordPass(in context);
		}
	}

	private static void RecordPass(in RecordContext context)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("DEBUG - Stencil", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\StencilDebug.cs", 46);
		passData.CameraColor = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraColor, AccessFlags.Write);
		passData.CameraDepth = context.FrameResources.CameraStackTargets.Depth;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraDepth);
		TextureDesc textureDesc = new TextureDesc(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		textureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		TextureDesc desc = textureDesc;
		passData.TempRT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData.Material = context.DebugContext.MaterialLibrary.FullscreenDebug;
		passData.StencilDebugPass = context.DebugContext.MaterialLibrary.StencilDebugPass;
		passData.DebugBlitPass = context.DebugContext.MaterialLibrary.DebugBlitPass;
		passData.StencilRef = context.DebugContext.DebugData.StencilDebug.Ref;
		passData.StencilFlags = context.DebugContext.DebugData.StencilDebug.Flags;
		passData.StencilDebugType = context.DebugContext.DebugData.StencilDebug.StencilDebugType;
		unsafeRenderGraphBuilder.SetRenderFunc<PassData>(ExecutePass);
	}

	private static void ExecutePass(PassData data, UnsafeGraphContext context)
	{
		switch (data.StencilDebugType)
		{
		case StencilDebugType.Flags:
			context.cmd.SetRenderTarget(data.TempRT, data.CameraDepth);
			context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
			context.cmd.SetGlobalFloat(ShaderIDs._Debug_StecilRef, (float)data.StencilFlags);
			context.cmd.SetGlobalFloat(ShaderIDs._Debug_StecilReadMask, (float)data.StencilFlags);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.StencilDebugPass, MeshTopology.Triangles, 3);
			context.cmd.SetRenderTarget(data.CameraColor);
			context.cmd.SetGlobalTexture(ShaderIDs._Debug_BlitInput, data.TempRT);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.DebugBlitPass, MeshTopology.Triangles, 3);
			break;
		case StencilDebugType.Ref:
			context.cmd.SetRenderTarget(data.TempRT, data.CameraDepth);
			context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
			context.cmd.SetGlobalFloat(ShaderIDs._Debug_StecilRef, data.StencilRef);
			context.cmd.SetGlobalFloat(ShaderIDs._Debug_StecilReadMask, 255f);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.StencilDebugPass, MeshTopology.Triangles, 3);
			context.cmd.SetRenderTarget(data.CameraColor);
			context.cmd.SetGlobalTexture(ShaderIDs._Debug_BlitInput, data.TempRT);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.DebugBlitPass, MeshTopology.Triangles, 3);
			break;
		case StencilDebugType.None:
			break;
		}
	}
}
