using System.Collections.Generic;

namespace Kingmaker.Framework.Utility;

public struct ScopedStack<T>
{
	private const int DefaultCapacity = 8;

	private List<T>? _stack;

	public ScopedStackFrame Push(T value)
	{
		if (_stack == null)
		{
			_stack = new List<T>(8);
		}
		_stack.Add(value);
		return new ScopedStackFrame(_stack);
	}

	public T? Peek()
	{
		List<T> stack = _stack;
		if (stack == null || stack.Count <= 0)
		{
			return default(T);
		}
		List<T>? stack2 = _stack;
		return stack2[stack2.Count - 1];
	}

	public void Clear()
	{
		_stack?.Clear();
	}
}
