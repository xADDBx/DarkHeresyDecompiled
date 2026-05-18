using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public struct GBufferResources
{
	public TextureHandle Albedo;

	public TextureHandle Specular;

	public TextureHandle Normals;

	public TextureHandle Translucency;

	public TextureHandle BakedGI;

	public TextureHandle Shadowmask;
}
