using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DebugMapOverlayPassData : PassDataBase
{
	public TextureHandle OverlayHandle;

	public float Size;

	public bool FlipVertically;
}
