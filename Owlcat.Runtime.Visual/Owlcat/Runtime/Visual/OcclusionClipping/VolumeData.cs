using Owlcat.Runtime.Visual.Experimental.Geometry;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal struct VolumeData
{
	public uint NodeId;

	public Obb Box;

	public int ClipCounter;
}
