using System;
using System.Threading.Tasks;
using Kingmaker.Signals;
using UnityEngine.Pool;

namespace Kingmaker.Code.Framework.Networking.Sync;

public sealed class AsyncSimulationScope : IAsyncDisposable
{
	private static readonly ObjectPool<AsyncSimulationScope> s_Pool = new ObjectPool<AsyncSimulationScope>(() => new AsyncSimulationScope());

	private SignalWrapper m_SignalWrapper = SignalWrapper.Empty;

	private bool _disposed;

	private AsyncSimulationScope()
	{
	}

	public static AsyncSimulationScope Get()
	{
		AsyncSimulationScope asyncSimulationScope = s_Pool.Get();
		asyncSimulationScope.m_SignalWrapper = SignalService.Instance.RegisterNext();
		return asyncSimulationScope;
	}

	public async ValueTask DisposeAsync()
	{
		if (!_disposed)
		{
			_disposed = true;
			while (!SignalService.Instance.CheckReadyOrSend(ref m_SignalWrapper))
			{
				await NextTickAwaiter.New();
			}
			s_Pool.Release(this);
		}
	}
}
