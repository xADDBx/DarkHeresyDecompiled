using System;
using Kingmaker.AreaLogic.Cutscenes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("f1bc0fd60e7b4e8a8716cdc9713e3110")]
public sealed class CommandWaitForElevatorPlatformTransitionEnd : CommandBase
{
	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
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
		return player.GetElevatorPlatform().IsIdle;
	}
}
