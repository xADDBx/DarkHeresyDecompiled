using System;
using System.Collections.Concurrent;
using Kingmaker.Controllers;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.Code.Framework.Networking.Sync;

public class SyncingService : IUpdatable, IService, IDisposable
{
	private static ServiceProxy<SyncingService> _proxy;

	private readonly object _lock = new object();

	private ConcurrentQueue<Action> _continuationsCurrent = new ConcurrentQueue<Action>();

	private ConcurrentQueue<Action> _continuationsNext = new ConcurrentQueue<Action>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public static SyncingService Instance
	{
		get
		{
			if (_proxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new SyncingService());
				_proxy = Services.GetProxy<SyncingService>();
			}
			return _proxy.Instance;
		}
	}

	private SyncingService()
	{
		Game.Instance.Controllers.CustomUpdateController.Add(this);
	}

	void IDisposable.Dispose()
	{
		Game.Instance.Controllers.CustomUpdateController.Remove(this);
	}

	void IUpdatable.Tick(float delta)
	{
		lock (_lock)
		{
			ConcurrentQueue<Action> continuationsNext = _continuationsNext;
			ConcurrentQueue<Action> continuationsCurrent = _continuationsCurrent;
			_continuationsCurrent = continuationsNext;
			_continuationsNext = continuationsCurrent;
		}
		Action result;
		while (_continuationsCurrent.TryDequeue(out result))
		{
			result();
		}
	}

	public void Schedule(Action continuation)
	{
		_continuationsNext.Enqueue(continuation);
	}
}
