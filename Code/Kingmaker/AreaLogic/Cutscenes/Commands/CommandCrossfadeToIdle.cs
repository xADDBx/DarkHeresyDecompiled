using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandCrossfadeToIdle")]
[TypeId("05168a5604dfb8346b09cce0559cba3e")]
public class CommandCrossfadeToIdle : CommandBase
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (Unit.TryGetValue(out var value))
		{
			value.View.Animator.CrossFadeInFixedTime("Idle", 0.1f);
			return CommandResult.Success;
		}
		return CommandResult.Fail("Unit not found");
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

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + "<b> force Idle</b> ";
	}
}
