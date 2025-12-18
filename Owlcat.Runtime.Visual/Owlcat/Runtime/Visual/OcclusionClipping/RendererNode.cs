using Owlcat.Runtime.Visual.Experimental.Geometry;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal struct RendererNode
{
	public int RendererId;

	public Obb Box;
}
