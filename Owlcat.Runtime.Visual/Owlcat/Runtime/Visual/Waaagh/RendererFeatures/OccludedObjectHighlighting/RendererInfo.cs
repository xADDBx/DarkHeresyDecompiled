using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting;

internal struct RendererInfo
{
	public OccludedObjectHighlighter Highlighter;

	public Renderer Renderer;

	public int ExpectedMaterialsCount;
}
