using Kingmaker.Blueprints.Attributes;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandCameraShake")]
[TypeId("03fc6437c7e02bb46bdb0bf5000e2209")]
public class CommandCameraShake : CommandBase
{
	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[HideIf("m_Continuous")]
	private float m_Lifetime = 1f;

	[SerializeField]
	private float m_Amplitude;

	[SerializeField]
	private float m_Speed;

	private bool m_Finished;

	public override bool IsContinuous => m_Continuous;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = skipping || m_Speed <= 0f || m_Amplitude < 0f || m_Lifetime <= 0f;
		if (!m_Finished)
		{
			CameraRig.Instance.StartShake(m_Amplitude, m_Speed);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		CameraRig.Instance.StopShake();
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
		return m_Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		m_Finished = m_Finished || (!m_Continuous && time >= (double)m_Lifetime);
		if (!m_Finished && !CameraRig.Instance.IsShaking())
		{
			CameraRig.Instance.StartShake(m_Amplitude, m_Speed);
		}
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "Shake camera";
	}
}
