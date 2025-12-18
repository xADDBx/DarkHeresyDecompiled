using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal struct WriteFillCommandData
{
	public int materialId;

	public int shapeId;

	public SurfaceCellFilterData selectFilter;
}
