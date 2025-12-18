using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("08339c119d39a394983f21f7f8fa8028")]
public class CommandDebugLog : CommandBase
{
	public string Text;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		PFLog.Default.Log("Command: " + Text);
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
