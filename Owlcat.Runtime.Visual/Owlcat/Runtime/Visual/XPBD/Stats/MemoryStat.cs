using Unity.Burst;

namespace Owlcat.Runtime.Visual.XPBD.Stats;

[BurstCompile]
public struct MemoryStat
{
	public int Cpu;

	public int Gpu;

	public static MemoryStat operator +(MemoryStat stat1, MemoryStat stat2)
	{
		MemoryStat result = default(MemoryStat);
		result.Cpu = stat1.Cpu + stat2.Cpu;
		result.Gpu = stat1.Gpu + stat2.Gpu;
		return result;
	}
}
