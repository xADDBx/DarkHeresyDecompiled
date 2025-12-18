using System;
using Kingmaker.AreaLogic.Cutscenes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("f1bc0fd60e7b4e8a8716cdc9713e3110")]
public sealed class CommandWaitForElevatorPlatformTransitionEnd : CommandBase
{
	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetElevatorPlatform().IsIdle;
	}
}
