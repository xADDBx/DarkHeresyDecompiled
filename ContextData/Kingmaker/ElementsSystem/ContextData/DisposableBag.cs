using System;
using System.Collections.Generic;

namespace Kingmaker.ElementsSystem.ContextData;

public class DisposableBag : IDisposable
{
	private static readonly Stack<DisposableBag> Pool = new Stack<DisposableBag>();

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>(8);

	private DisposableBag()
	{
	}

	public static DisposableBag Claim(IDisposable d1)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new DisposableBag();
		}
		result.m_Disposables.Add(d1);
		return result;
	}

	public static DisposableBag Claim(IDisposable d1, IDisposable d2)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new DisposableBag();
		}
		result.m_Disposables.Add(d1);
		result.m_Disposables.Add(d2);
		return result;
	}

	public static DisposableBag Claim(IDisposable d1, IDisposable d2, IDisposable d3)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new DisposableBag();
		}
		result.m_Disposables.Add(d1);
		result.m_Disposables.Add(d2);
		result.m_Disposables.Add(d3);
		return result;
	}

	public static DisposableBag Claim(IDisposable d1, IDisposable d2, IDisposable d3, IDisposable d4)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new DisposableBag();
		}
		result.m_Disposables.Add(d1);
		result.m_Disposables.Add(d2);
		result.m_Disposables.Add(d3);
		result.m_Disposables.Add(d4);
		return result;
	}

	public static DisposableBag Claim(params IDisposable[] disposables)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new DisposableBag();
		}
		foreach (IDisposable item in disposables)
		{
			result.m_Disposables.Add(item);
		}
		return result;
	}

	public void Dispose()
	{
		foreach (IDisposable disposable in m_Disposables)
		{
			disposable?.Dispose();
		}
		m_Disposables.Clear();
		Pool.Push(this);
	}
}
