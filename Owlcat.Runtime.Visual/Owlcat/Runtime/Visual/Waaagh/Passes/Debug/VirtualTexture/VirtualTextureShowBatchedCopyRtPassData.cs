using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowBatchedCopyRtPassData : PassDataBase
{
	public Material Material;

	public int Pass;

	public VirtualTextureManager VirtualTextureManager;

	public TextureHandle CameraFinalTarget;

	public bool ShowBatchedCopyRt;

	public float Scale;

	public float2 ScreenSize;

	public RTHandle BatchedCopyRt;

	public Texture2D BatchedCopyDebugTexture;

	public int Limit;

	public bool FlipVertically;
}
