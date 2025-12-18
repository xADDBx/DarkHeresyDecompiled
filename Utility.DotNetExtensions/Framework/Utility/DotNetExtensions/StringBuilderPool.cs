using System.Text;
using UnityEngine.Pool;

namespace Framework.Utility.DotNetExtensions;

public static class StringBuilderPool
{
	private const int Capacity = 256;

	private static readonly ObjectPool<StringBuilder> _Pool = new ObjectPool<StringBuilder>(() => new StringBuilder(256), null, delegate(StringBuilder l)
	{
		l.Clear();
	});

	public static StringBuilder Get()
	{
		return _Pool.Get();
	}

	public static PooledObject<StringBuilder> Get(out StringBuilder value)
	{
		return _Pool.Get(out value);
	}

	public static void Release(StringBuilder toRelease)
	{
		_Pool.Release(toRelease);
	}
}
