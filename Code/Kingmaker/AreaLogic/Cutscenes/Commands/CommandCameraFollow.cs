using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandCameraFollow")]
[TypeId("dd3e509d1d5d21c4b851a4d373822c4c")]
public class CommandCameraFollow : CommandBase
{
	[SerializeReference]
	public PositionEvaluator Target;

	public float OverrideRubberband = 3f;

	private float? m_OldRubberband;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Target.TryGetValue(out var _))
		{
			return CommandResult.Fail("Failed to find target");
		}
		m_OldRubberband = CameraRig.Instance.ScrollRubberBand;
		CameraRig.Instance.ScrollRubberBand = OverrideRubberband;
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (Target != null && Target.TryGetValue(out var value))
		{
			CameraRig.Instance.ScrollTo(value);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if (m_OldRubberband.HasValue)
		{
			CameraRig.Instance.ScrollRubberBand = m_OldRubberband.Value;
		}
		m_OldRubberband = null;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "Camera follows " + Target;
	}
}
