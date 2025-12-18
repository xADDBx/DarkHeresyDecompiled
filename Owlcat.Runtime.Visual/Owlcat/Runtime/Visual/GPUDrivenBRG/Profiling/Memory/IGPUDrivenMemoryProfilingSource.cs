namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;

public interface IGPUDrivenMemoryProfilingSource
{
	void FillMemoryCounters(Counters.CounterCollection counters);
}
