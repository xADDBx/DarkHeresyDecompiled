using System;
using Kingmaker.AreaLogic.Cutscenes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[TypeId("f7336680323e4a539e62681552fedb1c")]
public sealed class CommandDisableDetectiveServoskullSpan : CommandBase
{
	public override bool IsContinuous => true;

	public override string GetCaption()
	{
		return "Disable Detective Servoskull";
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull.Retain();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull.Release();
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
