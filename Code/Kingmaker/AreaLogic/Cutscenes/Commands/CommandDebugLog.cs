using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("08339c119d39a394983f21f7f8fa8028")]
public class CommandDebugLog : CommandBase
{
	public string Text;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		PFLog.Default.Log("Command: " + Text);
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}
}
