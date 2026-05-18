using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Interfaces.Canvas;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Assets.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("9e8e1aa656d843f43b18669c40b50724")]
public class CommandCustomLoadingScreen : CommandBase
{
	[ValidateNotNull]
	[AssetPicker("")]
	public GameObject Prefab;

	public float ShowTime = 1f;

	public ActionList Actions;

	private float m_TimeLeft;

	private bool m_Finished;

	private ICanvasAnimation m_CanvasAnimation;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = skipping;
		if (!skipping)
		{
			m_TimeLeft = ShowTime;
			GameObject arg = SimpleBlueprint.Instantiate(Prefab);
			m_CanvasAnimation = CanvasService.CreateCanvasAnimation?.Invoke(arg);
			LoadingProcess.Instance.ShowManualLoadingScreen(m_CanvasAnimation);
		}
		else
		{
			Actions.Run();
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		m_Finished = true;
		Actions.Run();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_CanvasAnimation == null;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		m_Finished = true;
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (m_CanvasAnimation == null)
		{
			return CommandResult.Success;
		}
		switch (m_CanvasAnimation.GetLoadingScreenState())
		{
		case LoadingScreenState.Shown:
			m_TimeLeft -= Time.unscaledDeltaTime;
			if (!m_Finished && m_TimeLeft <= 0f)
			{
				Actions.Run();
				LoadingProcess.Instance.HideManualLoadingScreen();
				m_Finished = true;
			}
			break;
		case LoadingScreenState.Hidden:
			UnityEngine.Object.Destroy(m_CanvasAnimation.GameObject);
			m_CanvasAnimation = null;
			break;
		}
		return CommandResult.Success;
	}
}
