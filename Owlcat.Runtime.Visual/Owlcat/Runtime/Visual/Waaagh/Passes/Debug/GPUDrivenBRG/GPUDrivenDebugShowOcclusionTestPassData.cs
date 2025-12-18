using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.GPUDrivenBRG;

public class GPUDrivenDebugShowOcclusionTestPassData : PassDataBase
{
	public TextureHandle CameraFinalTarget;

	public Material Material;

	public int Pass;
}
