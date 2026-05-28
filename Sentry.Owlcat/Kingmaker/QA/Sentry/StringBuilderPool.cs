using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Kingmaker.QA.Sentry;

internal sealed class StringBuilderPool : IStringBuilderPool
{
	private readonly ConcurrentQueue<StringBuilder> _pool = new ConcurrentQueue<StringBuilder>();

	private readonly int _maxCapacity;

	private StringBuilder _fastAccess;

	private int _currentCapacity;

	private StringBuilderPool(int maxCapacity)
	{
		_maxCapacity = maxCapacity;
	}

	public PooledStringBuilder Rent()
	{
		StringBuilder result = Interlocked.Exchange(ref _fastAccess, null);
		if (result != null)
		{
			return new PooledStringBuilder(result, this);
		}
		if (_pool.TryDequeue(out result))
		{
			Interlocked.Decrement(ref _currentCapacity);
			return new PooledStringBuilder(result, this);
		}
		return new PooledStringBuilder(new StringBuilder(), this);
	}

	public void Return(StringBuilder sb)
	{
		if (sb != null && Interlocked.CompareExchange(ref _fastAccess, sb, null) != null)
		{
			if (Interlocked.Increment(ref _currentCapacity) <= _maxCapacity)
			{
				_pool.Enqueue(sb);
			}
			else
			{
				Interlocked.Decrement(ref _currentCapacity);
			}
		}
	}

	public static IStringBuilderPool Create(int maxCapacity = 64)
	{
		return new StringBuilderPool(maxCapacity);
	}
}
