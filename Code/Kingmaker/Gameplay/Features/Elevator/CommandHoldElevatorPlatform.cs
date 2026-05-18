using System;
using Kingmaker.AreaLogic.Cutscenes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("fedd4981674d492995cdbf6878f674dc")]
public sealed class CommandHoldElevatorPlatform : CommandBase
{
	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		player.GetElevatorPlatform().CutsceneHold.Retain();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		player.GetElevatorPlatform().CutsceneHold.Release();
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
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

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}
}
