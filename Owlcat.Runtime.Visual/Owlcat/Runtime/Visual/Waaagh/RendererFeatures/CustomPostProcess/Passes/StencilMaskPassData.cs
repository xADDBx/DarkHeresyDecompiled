using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess.Passes;

public sealed class StencilMaskPassData : PassDataBase
{
	public CompareFunction CompareFunction;

	public Material Material;

	public StencilRef ReadMask;

	public StencilRef Ref;
}
