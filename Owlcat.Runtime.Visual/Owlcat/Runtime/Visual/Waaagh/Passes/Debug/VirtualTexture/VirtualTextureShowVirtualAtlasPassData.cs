using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowVirtualAtlasPassData : PassDataBase
{
	public Material Material;

	public int Pass;

	public float Scale;

	public float2 ScreenSize;

	public BufferHandle RectanglesBuffer;

	public VirtualTextureManager VirtualTextureManager;

	public NativeList<int4> Rects;

	public int UsedRectsCount;

	public int FreeRectsCount;

	public bool FlipVertically;
}
