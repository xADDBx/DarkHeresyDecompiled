using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawColorPyramidPass : ScriptableRenderPass<DrawColorPyramidPassData>
{
	private string m_Name;

	private Material m_ColorPyramidMaterial;

	private Material m_BlitMaterial;

	private ColorPyramidType m_Type;

	public override string Name => m_Name;

	private protected override WaaaghProfileId? ProfileId { get; }

	public DrawColorPyramidPass(RenderPassEvent evt, ColorPyramidType type, Material colorPyramidMaterial, Material blitMaterial)
		: base(evt)
	{
		m_ColorPyramidMaterial = colorPyramidMaterial;
		m_BlitMaterial = blitMaterial;
		m_Type = type;
		m_Name = string.Format("{0}.{1}", "DrawColorPyramidPass", type);
		ProfileId = type switch
		{
			ColorPyramidType.OpaqueDistortion => WaaaghProfileId.DrawColorPyramidPass_OpaqueDistortion, 
			ColorPyramidType.TransparentDistortion => WaaaghProfileId.DrawColorPyramidPass_TransparentDistortion, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		switch (m_Type)
		{
		case ColorPyramidType.OpaqueDistortion:
			DependsOn(in waaaghRendererListData.OpaqueDistortionGBuffer.List);
			break;
		case ColorPyramidType.TransparentDistortion:
			DependsOn(in waaaghRendererListData.DistortionVectors.List);
			break;
		}
	}

	protected override void Setup(RenderGraphBuilder builder, DrawColorPyramidPassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		frameData.Get<WaaaghRendererListData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.Input = builder.ReadWriteTexture(in input);
		data.Output = builder.ReadWriteTexture(in waaaghResourceData.CameraColorPyramidRT);
		data.TextureSize = new int2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
		data.BlitMaterial = m_BlitMaterial;
		data.ColorPyramidMaterial = m_ColorPyramidMaterial;
	}

	public override bool HasAnyCustomDependencyThatPreventsPassCulling(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		if (m_Type == ColorPyramidType.OpaqueDistortion)
		{
			if (!waaaghCameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
			{
				return WaaaghPipeline.Asset.SupportsCameraOpaqueTexture;
			}
			return component.RequiresColorTexture;
		}
		return false;
	}

	protected override void Render(DrawColorPyramidPassData data, RenderGraphContext context)
	{
		int num = 0;
		int num2 = data.TextureSize.x;
		int num3 = data.TextureSize.y;
		Vector4 value = new Vector4(1f, 1f, 0f, 0f);
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Input);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetRenderTarget(data.Output, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, num2, num3));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		while (num2 >= 8 || num3 >= 8)
		{
			int num4 = math.max(1, num2 >> 1);
			int num5 = math.max(1, num3 >> 1);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.Output);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(1f, 1f, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(1f, 1f, 2f / (float)num2, 0f));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, num);
			context.cmd.SetRenderTarget(data.Input, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num4, num5));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
			float x = (float)num4 / (float)data.TextureSize.x;
			float y = (float)num5 / (float)data.TextureSize.y;
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.Input);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(x, y, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num4 - 0.5f) / (float)data.TextureSize.x, ((float)num5 - 0.5f) / (float)data.TextureSize.y, 0f, 1f / (float)data.TextureSize.y));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
			context.cmd.SetRenderTarget(data.Output, num + 1);
			context.cmd.SetViewport(new Rect(0f, 0f, num4, num5));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
			num++;
			num2 >>= 1;
			num3 >>= 1;
		}
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Output);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetRenderTarget(data.Input, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, data.TextureSize.x, data.TextureSize.y));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorPyramidRT, data.Output);
		context.cmd.SetGlobalFloat(ShaderPropertyId._ColorPyramidLodCount, num);
	}
}
