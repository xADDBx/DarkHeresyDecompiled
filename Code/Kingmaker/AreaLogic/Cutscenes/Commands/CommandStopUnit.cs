using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandStopUnit")]
[TypeId("bf083819ef1b6fc4f8a1dcc5106710d8")]
public class CommandStopUnit : CommandBase
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (Unit.TryGetValue(out var value))
		{
			value.Commands.InterruptAllInterruptible();
			return CommandResult.Success;
		}
		return CommandResult.Fail("Failed to find unit");
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
		if (Unit == null)
		{
			return "<b>Stop</b> none";
		}
		return "<b>Stop</b> " + Unit?.GetCaptionShort();
	}

	public override string GetWarning()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			return null;
		}
		return "No unit";
	}
}
