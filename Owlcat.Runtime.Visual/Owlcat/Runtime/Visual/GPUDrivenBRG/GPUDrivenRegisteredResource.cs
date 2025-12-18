using System;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct GPUDrivenRegisteredResource<TKey> where TKey : unmanaged, IEquatable<TKey>
{
	public TKey Key;

	public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;
}
