using System;
using System.Collections;

namespace Kingmaker.Framework.Utility;

public readonly struct ScopedStackFrame : IDisposable
{
	private readonly IList _stack;

	private readonly int _index;

	public ScopedStackFrame(IList stack)
	{
		_stack = stack;
		_index = stack.Count - 1;
	}

	public void Dispose()
	{
		if (_index != _stack.Count - 1)
		{
			throw new InvalidOperationException("ScopedStackFrame: index mismatch");
		}
		_stack.RemoveAt(_index);
	}
}
