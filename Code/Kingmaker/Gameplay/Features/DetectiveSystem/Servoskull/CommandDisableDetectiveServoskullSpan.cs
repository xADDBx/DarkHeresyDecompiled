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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull.Retain();
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull.Release();
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
