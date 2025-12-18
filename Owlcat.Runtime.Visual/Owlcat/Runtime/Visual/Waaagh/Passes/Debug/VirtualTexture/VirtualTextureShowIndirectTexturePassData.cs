using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowIndirectTexturePassData : PassDataBase
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
