using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CopyDepthPass : ScriptableRasterPass<CopyDepthPassData>
{
	public enum CopyDepthMode
	{
		Final,
		Intermediate
	}

	public enum PassCullingCriteria
	{
		None,
		Opaque,
		OpaqueDistortion
	}

	private Material m_Material;

	private CopyDepthMode m_Mode;

	private Vector4 m_DepthPyramidSamplingRatio;

	private PassCullingCriteria m_CullingCriteria;

	public override string Name => "CopyDepthPass";

	public CopyDepthPass(RenderPassEvent evt, Material material, CopyDepthMode copyDepthMode, PassCullingCriteria cullingCriteria)
		: base(evt)
	{
		m_Material = material;
		m_Mode = copyDepthMode;
		m_DepthPyramidSamplingRatio = new Vector4(1f, 1f, 0f, 0f);
		m_CullingCriteria = cullingCriteria;
	}

	protected override void Setup(IRasterRenderGraphBuilder builder, CopyDepthPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		frameData.Get<WaaaghCameraData>();
		switch (m_Mode)
		{
		case CopyDepthMode.Final:
		{
			data.Input = waaaghResourceData.CameraDepthBuffer;
			TextureHandle input = waaaghResourceData.CameraDepthBuffer;
			builder.UseTexture(in input);
			builder.SetRenderAttachment(waaaghResourceData.CameraResolveDepthBuffer, 0);
			data.ShaderPass = 1;
			data.SetGlobalTexture = false;
			break;
		}
		case CopyDepthMode.Intermediate:
		{
			data.Input = waaaghResourceData.CameraDepthBuffer;
			TextureHandle input = waaaghResourceData.CameraDepthBuffer;
			builder.UseTexture(in input);
			builder.SetRenderAttachment(waaaghResourceData.CameraDepthCopyRT, 0);
			builder.SetGlobalTextureAfterPass(in waaaghResourceData.CameraDepthCopyRT, ShaderPropertyId._CameraDepthTexture);
			builder.SetGlobalTextureAfterPass(in waaaghResourceData.CameraDepthCopyRT, ShaderPropertyId._CameraDepthRT);
			data.ShaderPass = 0;
			data.SetGlobalTexture = true;
			break;
		}
		}
		builder.AllowGlobalStateModification(value: true);
		data.DepthPyramidSamplingRatio = m_DepthPyramidSamplingRatio;
		data.Material = m_Material;
	}

	protected override void Render(CopyDepthPassData data, RasterGraphContext context)
	{
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.Input);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.ShaderPass, MeshTopology.Triangles, 3);
	}
}
