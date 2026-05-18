using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandAddFogOfWarRevealer")]
[TypeId("72655d3c035284e418bff927ec00c094")]
public class CommandAddFogOfWarRevealer : CommandBase
{
	[SerializeField]
	[SerializeReference]
	private TransformEvaluator m_Revealer;

	private Transform m_EvaluatedRevealer;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (m_Revealer.TryGetValue(out m_EvaluatedRevealer))
		{
			FogOfWarControllerData.AddRevealer(m_EvaluatedRevealer);
			return CommandResult.Success;
		}
		return CommandResult.Fail("Failed to find revealer transform");
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if (m_EvaluatedRevealer != null)
		{
			FogOfWarControllerData.RemoveRevealer(m_EvaluatedRevealer);
			m_EvaluatedRevealer = null;
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
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

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "<b>Fog of war revealer</b>";
	}
}
