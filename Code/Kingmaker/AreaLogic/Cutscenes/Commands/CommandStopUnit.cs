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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Unit.GetValue()?.Commands.InterruptAllInterruptible();
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
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
