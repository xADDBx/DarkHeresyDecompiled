using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

internal struct RendererInfo
{
	public Highlighter Highlighter;

	public Renderer Renderer;

	public int ExpectedMaterialsCount;
}
