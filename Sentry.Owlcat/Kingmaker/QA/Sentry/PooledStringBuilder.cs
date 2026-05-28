using System;
using System.Text;

namespace Kingmaker.QA.Sentry;

internal struct PooledStringBuilder : IDisposable
{
	private readonly StringBuilderPool _pool;

	private bool _disposed;

	public StringBuilder Builder { get; }

	public PooledStringBuilder(StringBuilder builder, StringBuilderPool pool)
	{
		Builder = builder;
		_pool = pool;
		_disposed = false;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			Builder.Clear();
			_pool.Return(Builder);
			_disposed = true;
		}
	}
}
