using System;
using System.Collections;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Utility.ManualCoroutines;

public static class YieldInstructions
{
	public static IEnumerator WaitForSecondsGameTime(float seconds)
	{
		TimeSpan targetTime = Game.Instance.Controllers.TimeController.GameTime + seconds.Seconds();
		while (Game.Instance.Controllers.TimeController.GameTime < targetTime)
		{
			yield return null;
		}
	}
}
