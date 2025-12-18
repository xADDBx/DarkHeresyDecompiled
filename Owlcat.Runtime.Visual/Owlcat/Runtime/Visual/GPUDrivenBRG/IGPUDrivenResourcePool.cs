using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public interface IGPUDrivenResourcePool<TKey> : IGPUDrivenMemoryProfilingSource where TKey : unmanaged, IEquatable<TKey>
{
	int Count { get; }

	GPUDrivenResourcePoolEnumerator<TKey> GetEnumerator();
}
