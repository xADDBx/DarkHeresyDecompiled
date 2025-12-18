using Unity.Burst;

namespace Owlcat.Runtime.Visual.XPBD.Stats;

[BurstCompile]
public struct MemoryStats
{
	public MemoryStat BodyAllocator;

	public MemoryStat ColliderAllocator;

	public MemoryStat ParticleAttachmentAllocator;

	public MemoryStat DeformerAllocator;

	public MemoryStat SolverImpl;

	public MemoryStat BroadphaseHashGrid;

	public MemoryStat GetSum()
	{
		return BodyAllocator + ColliderAllocator + ParticleAttachmentAllocator + DeformerAllocator + SolverImpl + BroadphaseHashGrid;
	}

	public static string MemoryToString(in int memorySize)
	{
		if (memorySize > 1048576)
		{
			return $"{(float)memorySize / 1024f / 1024f:0.00}mb";
		}
		return $"{(float)memorySize / 1024f:0.00}kb";
	}
}
