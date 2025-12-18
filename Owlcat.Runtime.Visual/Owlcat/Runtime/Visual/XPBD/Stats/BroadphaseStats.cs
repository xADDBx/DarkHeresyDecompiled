using Unity.Burst;

namespace Owlcat.Runtime.Visual.XPBD.Stats;

[BurstCompile]
public struct BroadphaseStats
{
	public float ObjectsSizeSum;

	public int ObjectsCount;

	public float GridSpacing;

	public int HashmapSize;

	public int OccupiedCellsCount;

	public float SpatialHashmapLoadFactor;

	public int ActiveColliderContactsCount;

	public float ParticleSizeSum;

	public int SimplexCount;

	public float SimplexGridSpacing;

	public int SimplexHashmapSize;

	public int SimplexOccupiedCellsCount;

	public float SimplexSpatialHashmapLoadFactor;

	public int ActiveParticleContactsCount;
}
