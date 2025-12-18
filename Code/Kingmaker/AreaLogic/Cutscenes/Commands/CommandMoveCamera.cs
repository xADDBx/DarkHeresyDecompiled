using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMoveCamera")]
[TypeId("4a2a2b09db659af49b23c329e7b1744b")]
public class CommandMoveCamera : CommandBase
{
	private class Data
	{
		public Vector3 OriginalTarget;

		public bool TakingTooLong;

		[CanBeNull]
		public Coroutine ScrollCoroutine;

		public TimeSpan TargetTime;
	}

	[SerializeReference]
	public PositionEvaluator Target;

	public bool Teleport;

	[HideIf("Teleport")]
	public float CameraSpeed;

	[HideIf("Teleport")]
	public bool FixedScrollTime;

	[ShowIf("FixedScrollTime")]
	public float CameraMaxSpeed;

	[ShowIf("FixedScrollTime")]
	public float CameraMaxTravelTime;

	public bool MoveToInitialPosition;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		CameraRig.Instance.SetWorldOffset(Vector2.zero);
		Vector3 vector;
		if (MoveToInitialPosition)
		{
			vector = player.CameraStartingPosition;
		}
		else
		{
			if (Target == null)
			{
				return;
			}
			vector = CameraRig.Instance.ClampByLevelBounds(Target.GetValue());
		}
		Game.Instance.Controllers.CameraController?.Follower?.Release();
		Data commandData = player.GetCommandData<Data>(this);
		commandData.OriginalTarget = vector;
		float targetTime = -1f;
		if (Teleport)
		{
			CameraRig.Instance.ScrollToImmediately(vector);
		}
		else if (FixedScrollTime)
		{
			commandData.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(vector, out targetTime, CameraMaxTravelTime, CameraMaxSpeed, CameraSpeed);
		}
		else if (CameraSpeed > 0f)
		{
			commandData.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(vector, out targetTime, 0f, 0f, CameraSpeed);
		}
		else
		{
			CameraRig.Instance.ScrollTo(vector);
		}
		if (0f < targetTime)
		{
			commandData.TargetTime = Game.Instance.Controllers.TimeController.GameTime + targetTime.Seconds();
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		CameraRig.Instance.StopCoroutine(commandData.ScrollCoroutine);
		commandData.ScrollCoroutine = null;
		commandData.TargetTime = TimeSpan.Zero;
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		Vector3 position = CameraRig.Instance.ClampByLevelBounds(Target.GetValue());
		CameraRig.Instance.ScrollToImmediately(position);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (!Target)
		{
			return true;
		}
		Data commandData = player.GetCommandData<Data>(this);
		if (!(commandData.TargetTime <= Game.Instance.Controllers.TimeController.GameTime))
		{
			return commandData.TakingTooLong;
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (time > 20.0)
		{
			player.GetCommandData<Data>(this).TakingTooLong = true;
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		if ((bool)Target)
		{
			Vector3 originalTarget = player.GetCommandData<Data>(this).OriginalTarget;
			CameraRig.Instance.ScrollTo(originalTarget);
		}
	}

	public override string GetCaption()
	{
		return "to " + (Target ? Target.GetCaption() : "???");
	}
}
