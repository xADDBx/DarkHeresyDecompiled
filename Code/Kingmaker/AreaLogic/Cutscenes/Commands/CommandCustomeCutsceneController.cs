using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandCustomeCutsceneController")]
[TypeId("1aa0e70617f68494785d51510dd019c5")]
public class CommandCustomeCutsceneController : CommandBase
{
	[ValidateNotNull]
	[SerializeReference]
	public ArtCutsceneControllerEvaluator CustomeControllerEvaluator;

	private bool m_IsVfxComplete;

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_IsVfxComplete;
	}

	public override string GetCaption()
	{
		return $"CutsceneController {CustomeControllerEvaluator}";
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		m_IsVfxComplete = false;
		if (CustomeControllerEvaluator.TryGetValue(out var value))
		{
			value.OnRun(OnComplete);
			return CommandResult.Success;
		}
		m_IsVfxComplete = true;
		return CommandResult.Fail("Failed to find CutsceneCamera on object");
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		m_IsVfxComplete = true;
		if (CustomeControllerEvaluator.TryGetValue(out var value))
		{
			value.OnStop();
			return CommandResult.Success;
		}
		return CommandResult.Fail("Failed to find CutsceneCamera on object");
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	private void OnComplete()
	{
		m_IsVfxComplete = true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}
}
