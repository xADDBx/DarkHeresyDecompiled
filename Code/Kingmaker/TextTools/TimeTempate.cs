using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class TimeTempate : TextTemplate
{
	public override int MaxParameters => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		string text = parameters.FirstOrDefault();
		if (text.IsNullOrEmpty())
		{
			text = "HH:MM";
		}
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		return text.Replace("HH", gameTime.Hours.ToString("D2")).Replace("MM", gameTime.Minutes.ToString("D2")).Replace("SS", gameTime.Seconds.ToString("D2"));
	}
}
