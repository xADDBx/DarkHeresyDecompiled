using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public struct VTFeedbackData
{
	public TextureHandle VTFeedback;

	public TextureHandle VTFeedbackMRT;

	public BufferHandle VTPackedFeedbackBuffer;
}
