using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.IndirectRendering.Details;

internal class NativeListWrapper<T> : IDisposable where T : unmanaged
{
	private NativeList<T> m_List;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_List.Length;
		}
	}

	public ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref UnsafeCollectionExtensions.ElementAsRef(in m_List, index);
		}
	}

	public NativeListWrapper(Allocator allocator)
	{
		m_List = new NativeList<T>(allocator);
	}

	public void Dispose()
	{
		if (m_List.IsCreated)
		{
			m_List.Dispose();
		}
	}

	~NativeListWrapper()
	{
		Dispose();
	}

	public void Clear()
	{
		m_List.Clear();
	}

	public void AddRange(NativeListWrapper<T> range)
	{
		m_List.AddRange(range.m_List.AsArray());
	}

	public NativeArray<T> AsArray()
	{
		return m_List.AsArray();
	}

	public NativeList<T> GetInnerList()
	{
		return m_List;
	}

	public void Add(T item)
	{
		m_List.Add(in item);
	}
}
