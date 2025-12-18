using System;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

[Serializable]
public struct StencilMaskTextureSettings
{
	public StencilRef Ref;

	public StencilRef ReadMask;

	public CompareFunction CompareFunction;

	public FilterMode FilterMode;
}
