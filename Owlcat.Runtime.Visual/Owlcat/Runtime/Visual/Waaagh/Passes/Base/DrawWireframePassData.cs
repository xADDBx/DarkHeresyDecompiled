using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class DrawWireframePassData : PassDataBase
{
	public Camera Camera;

	public RendererListHandle RendererListHdl;
}
