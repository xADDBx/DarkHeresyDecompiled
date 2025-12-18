using Unity.Burst;

namespace Owlcat.Runtime.Visual.XPBD.Stats;

[BurstCompile]
public struct MainStats
{
	public MemoryStats MemoryStats;

	public BroadphaseStats BroadphaseStats;
}
