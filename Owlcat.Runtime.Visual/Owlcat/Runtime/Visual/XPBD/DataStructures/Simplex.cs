using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
public struct Simplex
{
	public int4 ParticleIndices;

	public int Size;

	public int IndexInSolver;

	public Aabb Aabb;

	public int BodyLayer;
}
