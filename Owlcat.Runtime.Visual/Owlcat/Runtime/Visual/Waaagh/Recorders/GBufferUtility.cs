using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GBufferUtility
{
	public static void ConfigureDrawPass(IRenderAttachmentRenderGraphBuilder builder, in FrameResources resources, bool isVTEnabled)
	{
		builder.SetRenderAttachment(resources.GBuffer.Albedo, 0);
		builder.SetRenderAttachment(resources.GBuffer.Specular, 1);
		builder.SetRenderAttachment(resources.GBuffer.Normals, 2);
		builder.SetRenderAttachment(resources.CameraStackTargets.Color, 3);
		builder.SetRenderAttachment(resources.GBuffer.Translucency, 4);
		builder.SetRenderAttachment(resources.GBuffer.BakedGI, 5);
		builder.SetRenderAttachment(resources.GBuffer.Shadowmask, 6);
		if (isVTEnabled)
		{
			builder.SetRenderAttachment(resources.VTFeedbackData.VTFeedbackMRT, 7);
		}
		builder.SetRenderAttachmentDepth(resources.CameraStackTargets.Depth, AccessFlags.Read);
	}
}
