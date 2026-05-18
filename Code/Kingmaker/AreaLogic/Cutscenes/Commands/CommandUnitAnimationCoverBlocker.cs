using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandUnitAnimationCoverBlocker")]
[TypeId("4394fb3648d14b5e95ab4111fc3fcff7")]
public class CommandUnitAnimationCoverBlocker : CommandBase
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool CoverAvailable;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		UnitAnimationManager animationManager = value.View.AnimationManager;
		if (animationManager == null)
		{
			return CommandResult.Fail("No unit animation manager");
		}
		animationManager.InCutsceneCoverAvailable = CoverAvailable;
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
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
}
