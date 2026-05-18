using System.Collections;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.Utility.ManualCoroutines;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/SmoothSlowMoCommand")]
[TypeId("c474d2c2537e6224b97a937efdb7b5fc")]
public class SmoothSlowMoCommand : CommandBase
{
	public float TargetScale = 0.1f;

	public float EaseInSeconds = 1f;

	public float HoldSeconds = 2f;

	public float EaseOutSeconds = 1f;

	private CoroutineHandler m_Coroutine;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!m_Coroutine.IsRunning)
		{
			m_Coroutine = Game.Instance.Controllers.CoroutinesController.Start(EaseSlowMo());
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		ResetScale();
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		ResetScale();
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		ResetScale();
		return CommandResult.Success;
	}

	private IEnumerator EaseSlowMo()
	{
		TimeController controller = Game.Instance.Controllers.TimeController;
		float startScale = 1f;
		float t = 0f;
		while (t < EaseInSeconds)
		{
			t += Game.Instance.RealTimeController.SystemDeltaTime;
			float t2 = Mathf.Clamp01(t / EaseInSeconds);
			controller.SlowMoTimeScale = Mathf.Lerp(startScale, TargetScale, EaseIn(t2));
			yield return null;
		}
		float holdT = 0f;
		while (holdT < HoldSeconds)
		{
			holdT += Game.Instance.RealTimeController.SystemDeltaTime;
			controller.SlowMoTimeScale = TargetScale;
			yield return null;
		}
		t = 0f;
		while (t < EaseOutSeconds)
		{
			t += Game.Instance.RealTimeController.SystemDeltaTime;
			float t3 = Mathf.Clamp01(t / EaseOutSeconds);
			controller.SlowMoTimeScale = Mathf.Lerp(TargetScale, startScale, EaseIn(t3));
			yield return null;
		}
		ResetScale();
	}

	private void ResetScale()
	{
		if (m_Coroutine.IsRunning)
		{
			Game.Instance.Controllers.CoroutinesController.Stop(m_Coroutine);
		}
		Game.Instance.Controllers.TimeController.SlowMoTimeScale = 1f;
	}

	private float EaseIn(float t)
	{
		return t * t;
	}
}
