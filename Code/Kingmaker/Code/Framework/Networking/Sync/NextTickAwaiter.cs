using System;
using System.Runtime.CompilerServices;

namespace Kingmaker.Code.Framework.Networking.Sync;

public readonly struct NextTickAwaiter : INotifyCompletion
{
	private readonly SyncingService _scheduler;

	public bool IsCompleted => false;

	private NextTickAwaiter(SyncingService scheduler)
	{
		_scheduler = scheduler;
	}

	public static NextTickAwaiter New()
	{
		return new NextTickAwaiter(SyncingService.Instance);
	}

	public void OnCompleted(Action continuation)
	{
		_scheduler.Schedule(continuation);
	}

	public void GetResult()
	{
	}

	public NextTickAwaiter GetAwaiter()
	{
		return this;
	}
}
