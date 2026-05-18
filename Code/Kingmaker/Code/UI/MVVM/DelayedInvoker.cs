using System;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public static class DelayedInvoker
{
	public static IDisposable InvokeInTime(Action action, float seconds)
	{
		return Observable.Timer(TimeSpan.FromSeconds(seconds), UnityTimeProvider.UpdateIgnoreTimeScale).Subscribe(action);
	}

	public static IDisposable InvokeInFrames(Action action, int frameCount)
	{
		return Observable.TimerFrame(frameCount).Subscribe(action);
	}

	public static IDisposable InvokeAtTheEndOfFrameOnlyOnes(Action action)
	{
		return Observable.TimerFrame(1, UnityFrameProvider.PostLateUpdate).Subscribe(action);
	}
}
