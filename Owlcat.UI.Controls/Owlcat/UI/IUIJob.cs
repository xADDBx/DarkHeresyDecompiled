using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace Owlcat.UI;

internal interface IUIJob
{
	private static readonly ObservableList<IUIJob> sQueue;

	private static readonly IDisposable sSubscription;

	private static bool sGuard;

	private static void Process(Unit _)
	{
		if (sGuard)
		{
			return;
		}
		sGuard = true;
		IUIJob element;
		while (TryDequeue(sQueue, out element))
		{
			try
			{
				element.Run();
			}
			catch (Exception message)
			{
				UIKitLogger.Error(message);
			}
		}
		sGuard = false;
	}

	private static bool TryDequeue<T>(IList<T> queue, out T element)
	{
		if (queue.Count == 0)
		{
			element = default(T);
			return false;
		}
		element = queue[0];
		queue.RemoveAt(0);
		return true;
	}

	static void Enqueue(IUIJob item)
	{
		if (!sQueue.Contains(item))
		{
			sQueue.Add(item);
		}
	}

	static void Remove(IUIJob item)
	{
		sQueue.Remove(item);
	}

	void Run();

	static IUIJob()
	{
		sQueue = new ObservableList<IUIJob>();
		sSubscription = sQueue.ObserveAdd().AsUnitObservable().DebounceFrame(0, UnityFrameProvider.PostLateUpdate)
			.Subscribe(Process);
		sGuard = false;
	}
}
