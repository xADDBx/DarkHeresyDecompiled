using System;
using Kingmaker.AreaLogic.Cutscenes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("fedd4981674d492995cdbf6878f674dc")]
public sealed class CommandHoldElevatorPlatform : CommandBase
{
	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		player.GetElevatorPlatform().CutsceneHold.Retain();
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		player.GetElevatorPlatform().CutsceneHold.Release();
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}
}
