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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Unit.GetValue().View.Animator.CrossFadeInFixedTime("Idle", 0.1f);
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
		return Unit?.GetCaptionShort() + "<b> force Idle</b> ";
	}
}
