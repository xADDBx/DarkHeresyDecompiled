using System;
using System.Collections;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.OBSOLETE;

[Obsolete]
public static class MainThreadDispatcher
{
	private class CoroutineRunner : MonoBehaviour
	{
	}

	public static bool IsInitialized;

	public static float FrequentUpdateInterval;

	public static float InfrequentUpdateInterval;

	private static CoroutineRunner s_CoroutineRunnerInstance;

	public static void Initialize()
	{
	}

	public static Observable<Unit> LateUpdateAsObservable()
	{
		return Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate);
	}

	public static Observable<Unit> UpdateAsObservable()
	{
		return Observable.EveryUpdate(UnityFrameProvider.Update);
	}

	public static Observable<Unit> FrequentUpdateAsObservable()
	{
		return Observable.EveryUpdate(UnityFrameProvider.Update);
	}

	public static Observable<Unit> InfrequentUpdateAsObservable()
	{
		return Observable.EveryUpdate(UnityFrameProvider.Update);
	}

	public static void Post(Action action)
	{
		Observable.TimerFrame(1).Subscribe(action);
	}

	public static void Send(Action<object> action, object state)
	{
		OwlcatR3UnitExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			action(state);
		});
	}

	public static void StartUpdateMicroCoroutine(IEnumerator routine)
	{
		StartCoroutine(routine);
	}

	public static Coroutine StartCoroutine(IEnumerator routine)
	{
		if (s_CoroutineRunnerInstance == null)
		{
			GameObject obj = new GameObject
			{
				name = "CoroutineRunner"
			};
			UnityEngine.Object.DontDestroyOnLoad(obj);
			s_CoroutineRunnerInstance = obj.AddComponent<CoroutineRunner>();
		}
		return s_CoroutineRunnerInstance.StartCoroutine(routine);
	}

	public static async void Send(Action action)
	{
		await Awaitable.MainThreadAsync();
		action();
	}
}
