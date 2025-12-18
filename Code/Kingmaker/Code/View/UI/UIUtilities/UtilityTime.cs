using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityTime
{
	public static void DoTaskLater(float timer, TweenCallback action, bool update = true)
	{
		if (Math.Abs(timer) < Mathf.Epsilon)
		{
			action?.Invoke();
			return;
		}
		DOTween.To(() => 1f, delegate
		{
		}, 0f, timer).SetUpdate(update).OnComplete(action);
	}

	public static string TimeSpanToInGameTime(TimeSpan timeSpan)
	{
		return string.Format(UIStrings.Instance.SaveLoadTexts.InGameFormat, (timeSpan.TotalDays >= 1.0) ? timeSpan.ToString("d' days 'hh':'mm") : timeSpan.ToString("hh':'mm"));
	}
}
