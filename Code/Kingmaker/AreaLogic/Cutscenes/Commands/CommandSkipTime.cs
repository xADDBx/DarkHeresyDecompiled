using System;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandSkipTime")]
[TypeId("08bbef9b0aa7870439a809872a6c8a88")]
public class CommandSkipTime : CommandBase
{
	private enum SkipType
	{
		Minutes,
		TimeOfDay,
		MinutesAndTime,
		TimeThenMinutes
	}

	[SerializeField]
	private SkipType m_Type;

	public Kingmaker.AreaLogic.TimeOfDay.TimeOfDay TimeOfDay;

	public int Minutes;

	public bool ReloadStaticIfNeeded;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		_ = Game.Instance.TimeOfDay;
		switch (m_Type)
		{
		case SkipType.Minutes:
			Game.Instance.AdvanceGameTime(Minutes.Minutes());
			break;
		case SkipType.TimeOfDay:
			Game.Instance.Controllers.TimeController.SkipGameTime(TimeOfDay);
			break;
		case SkipType.MinutesAndTime:
			Game.Instance.AdvanceGameTime(Minutes.Minutes());
			Game.Instance.MatchTimeOfDayForced();
			Game.Instance.Controllers.TimeController.SkipGameTime(TimeOfDay);
			break;
		case SkipType.TimeThenMinutes:
			Game.Instance.Controllers.TimeController.SkipGameTime(TimeOfDay);
			Game.Instance.AdvanceGameTime(Minutes.Minutes());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (ReloadStaticIfNeeded)
		{
			Game.Instance.MatchTimeOfDay();
		}
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return m_Type switch
		{
			SkipType.Minutes => "<b>Time</b> +" + Minutes + " min", 
			SkipType.TimeOfDay => "<b>Wait</b> till " + TimeOfDay, 
			SkipType.MinutesAndTime => "<b>Time</b> +" + Minutes + " and till " + TimeOfDay, 
			SkipType.TimeThenMinutes => "<b>Wait</b> till " + TimeOfDay.ToString() + " and " + Minutes + " min", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
