using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingSetupForRenderPassData : PassDataBase
{
	public TerrainStampingConstantBuffer ConstantBuffer;

	public Texture Normals;

	public Texture Texture;
}
