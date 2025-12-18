using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct GPUDrivenResourcePoolEnumerator<TKey> : IEnumerator<GPUDrivenRegisteredResource<TKey>>, IEnumerator, IDisposable where TKey : unmanaged, IEquatable<TKey>
{
	private NativeHashMap<TKey, GPUDrivenIndexAllocator.IndexAllocation>.Enumerator m_Enumerator;

	public GPUDrivenRegisteredResource<TKey> Current
	{
		get
		{
			KVPair<TKey, GPUDrivenIndexAllocator.IndexAllocation> current = m_Enumerator.Current;
			GPUDrivenRegisteredResource<TKey> result = default(GPUDrivenRegisteredResource<TKey>);
			result.Key = current.Key;
			result.IndexAllocation = current.Value;
			return result;
		}
	}

	object IEnumerator.Current => Current;

	public GPUDrivenResourcePoolEnumerator(NativeHashMap<TKey, GPUDrivenIndexAllocator.IndexAllocation> hashMap)
	{
		m_Enumerator = hashMap.GetEnumerator();
	}

	public void Dispose()
	{
		m_Enumerator.Dispose();
	}

	public bool MoveNext()
	{
		return m_Enumerator.MoveNext();
	}

	public void Reset()
	{
		m_Enumerator.Reset();
	}
}
