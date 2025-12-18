using System;
using JetBrains.Annotations;

namespace Kingmaker.ElementsSystem.ContextData;

public abstract class SimpleContextData<TValue, TSelf> : ContextData<TSelf> where TSelf : SimpleContextData<TValue, TSelf>, new()
{
	[CanBeNull]
	private TValue m_Value;

	[CanBeNull]
	public new static TValue Current
	{
		get
		{
			TSelf current = ContextData<TSelf>.Current;
			if (current == null)
			{
				return default(TValue);
			}
			return current.m_Value;
		}
	}

	[CanBeNull]
	public new static TValue Top
	{
		get
		{
			TSelf top = ContextData<TSelf>.Top;
			if (top == null)
			{
				return default(TValue);
			}
			return top.m_Value;
		}
	}

	public static bool TryGetCurrent(out TValue result)
	{
		TSelf current = ContextData<TSelf>.Current;
		if (current != null)
		{
			result = current.m_Value;
			return true;
		}
		result = default(TValue);
		return false;
	}

	[NotNull]
	public static IDisposable Set([NotNull] TValue value)
	{
		TSelf val = ContextData<TSelf>.Request();
		val.m_Value = value;
		return val;
	}

	[CanBeNull]
	public static IDisposable SetIfNotNull([CanBeNull] TValue value)
	{
		if (value == null)
		{
			return null;
		}
		return Set(value);
	}

	protected override void Reset()
	{
		m_Value = default(TValue);
	}
}
