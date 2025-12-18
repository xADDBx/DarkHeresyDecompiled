using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureFeedbackDebugPassData : PassDataBase
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
