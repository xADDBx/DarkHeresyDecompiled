using Owlcat.Runtime.Visual.Experimental.Geometry;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal struct VolumeNode
{
	public int VolumeId;

	public Obb Box;
}
