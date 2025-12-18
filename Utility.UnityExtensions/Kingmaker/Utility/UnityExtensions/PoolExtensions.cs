using System.Collections.Generic;
using UnityEngine.Pool;

namespace Kingmaker.Utility.UnityExtensions;

public static class PoolExtensions
{
	public static PooledObject<List<T>> ToPooledList<T>(this IEnumerable<T> source, out List<T> list)
	{
		PooledObject<List<T>> result = CollectionPool<List<T>, T>.Get(out list);
		list.AddRange(source);
		return result;
	}
}
