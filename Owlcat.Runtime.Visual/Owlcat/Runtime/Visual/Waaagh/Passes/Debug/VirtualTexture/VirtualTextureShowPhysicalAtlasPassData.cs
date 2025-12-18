using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowPhysicalAtlasPassData : PassDataBase
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
