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

[ComponentName("Command/CommandControlCamera")]
[TypeId("955b28ccab87414899465e01b0412d33")]
public class CommandControlCamera : CommandBase
{
	public enum TimingModeType
	{
		FixSpeed,
		FixTime,
		Snap
	}

	private class Data
	{
		public bool TakingTooLong;

		public TimeSpan TargetTime;

		[CanBeNull]
		public Coroutine ScrollCoroutine;

		[CanBeNull]
		public Coroutine RotateCoroutine;
	}

	public TimingModeType TimingMode;

	[ShowIf("TimingIsTime")]
	public float FixedTime;

	public bool Move;

	[ShowIf("Move")]
	[SerializeReference]
	public PositionEvaluator MoveTarget;

	[ShowIf("Move")]
	[SerializeReference]
	public bool MoveToInitialCameraPosition;

	[ShowIf("ShowMoveSpeed")]
	public float MoveSpeed;

	[ShowIf("ShowMoveCurve")]
	public AnimationCurve MoveCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public bool Rotate;

	[ShowIf("Rotate")]
	[SerializeReference]
	public FloatEvaluator RotateTarget;

	[ShowIf("Rotate")]
	[SerializeReference]
	public bool RotateToInitialCameraPosition;

	[ShowIf("ShowRotateSpeed")]
	public float RotateSpeed;

	[ShowIf("ShowRotateCurve")]
	public AnimationCurve RotateCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public bool Zoom;

	[ShowIf("Zoom")]
	[Range(0f, 1f)]
	[Tooltip("0 = далеко\n1 = близко")]
	public float ZoomTarget;

	[ShowIf("ShowZoomSpeed")]
	public float ZoomSpeed;

	[ShowIf("ShowZoomCurve")]
	public AnimationCurve ZoomCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private bool ShowMoveSpeed
	{
		get
		{
			if (Move)
			{
				return TimingMode == TimingModeType.FixSpeed;
			}
			return false;
		}
	}

	private bool ShowRotateSpeed
	{
		get
		{
			if (Rotate)
			{
				return TimingMode == TimingModeType.FixSpeed;
			}
			return false;
		}
	}

	private bool ShowZoomSpeed
	{
		get
		{
			if (Zoom)
			{
				return TimingMode == TimingModeType.FixSpeed;
			}
			return false;
		}
	}

	private bool ShowMoveCurve
	{
		get
		{
			if (Move)
			{
				if (TimingMode != 0 || !(MoveSpeed > 0f))
				{
					return TimingIsTime;
				}
				return true;
			}
			return false;
		}
	}

	private bool ShowRotateCurve
	{
		get
		{
			if (Rotate)
			{
				if (TimingMode != 0 || !(RotateSpeed > 0f))
				{
					return TimingIsTime;
				}
				return true;
			}
			return false;
		}
	}

	private bool ShowZoomCurve
	{
		get
		{
			if (Zoom)
			{
				if (TimingMode != 0 || !(ZoomSpeed > 0f))
				{
					return TimingIsTime;
				}
				return true;
			}
			return false;
		}
	}

	private bool TimingIsTime => TimingMode == TimingModeType.FixTime;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		TimingModeType timingMode = (skipping ? TimingModeType.Snap : TimingMode);
		CameraRig.Instance?.SetWorldOffset(Vector2.zero);
		StartCameraTransformations(player, commandData, timingMode);
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		CameraRig instance = CameraRig.Instance;
		instance.StopCoroutine(commandData.RotateCoroutine);
		instance.StopCoroutine(commandData.ScrollCoroutine);
		commandData.RotateCoroutine = null;
		commandData.ScrollCoroutine = null;
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		StartCameraTransformations(player, commandData, TimingModeType.Snap);
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.TakingTooLong)
		{
			return true;
		}
		if (TimeSpan.Zero < commandData.TargetTime)
		{
			return commandData.TargetTime <= Game.Instance.Controllers.TimeController.GameTime;
		}
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (time > 20.0)
		{
			player.GetCommandData<Data>(this).TakingTooLong = true;
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		StartCameraTransformations(player, commandData, TimingModeType.Snap);
		commandData.RotateCoroutine = null;
		commandData.ScrollCoroutine = null;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		if (Move)
		{
			return " to " + (MoveToInitialCameraPosition ? "initial pos" : (MoveTarget ? MoveTarget.GetCaption() : "???"));
		}
		if (Rotate)
		{
			return "<b>Rotate </b> to " + (RotateToInitialCameraPosition ? "initial pos" : (RotateTarget ? RotateTarget.GetCaption() : "???"));
		}
		if (Zoom)
		{
			return "<b>Zoom </b> to " + ZoomTarget;
		}
		return "Control camera";
	}

	private void StartCameraTransformations(CutscenePlayerData player, Data data, TimingModeType timingMode)
	{
		data.TargetTime = TimeSpan.Zero;
		Game.Instance.Controllers.CameraController?.Follower?.Release();
		if (Move)
		{
			if (MoveToInitialCameraPosition)
			{
				MoveCamera(player.CameraStartingPosition, data, timingMode);
			}
			else if (MoveTarget != null)
			{
				MoveCamera(CameraRig.Instance.ClampByLevelBounds(MoveTarget.GetValue()), data, timingMode);
			}
		}
		if (Rotate)
		{
			if (MoveToInitialCameraPosition)
			{
				RotateCamera(player.CameraStartingRotation, data, timingMode);
			}
			else if (RotateTarget != null)
			{
				float tgt = RotateTarget.GetValue() + 180f;
				RotateCamera(tgt, data, timingMode);
			}
		}
		if (Zoom)
		{
			ZoomCamera(ZoomTarget, data, timingMode);
		}
	}

	private void MoveCamera(Vector3 tgt, Data data, TimingModeType timingMode)
	{
		switch (timingMode)
		{
		case TimingModeType.FixSpeed:
			if (MoveSpeed > 0f)
			{
				data.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(tgt, out var targetTime, 0f, float.MaxValue, MoveSpeed, MoveCurve);
				TimeSpan timeSpan = Game.Instance.Controllers.TimeController.GameTime + targetTime.Seconds();
				data.TargetTime = ((timeSpan > data.TargetTime) ? timeSpan : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.FixTime:
			if (FixedTime > 0f)
			{
				data.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(tgt, out var targetTime2, FixedTime, float.MaxValue, 0f, MoveCurve);
				TimeSpan timeSpan2 = Game.Instance.Controllers.TimeController.GameTime + targetTime2.Seconds();
				data.TargetTime = ((timeSpan2 > data.TargetTime) ? timeSpan2 : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.Snap:
			if (data.ScrollCoroutine != null)
			{
				CameraRig.Instance.StopCoroutine(data.ScrollCoroutine);
				data.ScrollCoroutine = null;
			}
			CameraRig.Instance.ScrollToImmediately(tgt);
			return;
		}
		CameraRig.Instance.ScrollTo(tgt);
	}

	private void RotateCamera(float tgt, Data data, TimingModeType timingMode)
	{
		switch (timingMode)
		{
		case TimingModeType.FixSpeed:
			if (RotateSpeed > 0f)
			{
				data.RotateCoroutine = CameraRig.Instance.RotateToTimed(tgt, out var targetTime2, 0f, RotateSpeed, RotateCurve);
				TimeSpan timeSpan2 = Game.Instance.Controllers.TimeController.GameTime + targetTime2.Seconds();
				data.TargetTime = ((timeSpan2 > data.TargetTime) ? timeSpan2 : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.FixTime:
			if (FixedTime > 0f)
			{
				data.RotateCoroutine = CameraRig.Instance.RotateToTimed(tgt, out var targetTime, FixedTime, 0f, RotateCurve);
				TimeSpan timeSpan = Game.Instance.Controllers.TimeController.GameTime + targetTime.Seconds();
				data.TargetTime = ((timeSpan > data.TargetTime) ? timeSpan : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.Snap:
			CameraRig.Instance.RotateToImmediately(tgt);
			return;
		}
		CameraRig.Instance.RotateTo(tgt);
	}

	private void ZoomCamera(float tgt, Data data, TimingModeType timingMode)
	{
		switch (timingMode)
		{
		case TimingModeType.FixSpeed:
			if (ZoomSpeed > 0f)
			{
				CameraRig.Instance.CameraZoom.ZoomToTimed(tgt, out var targetTime2, 0f, ZoomSpeed, ZoomCurve);
				TimeSpan timeSpan2 = Game.Instance.Controllers.TimeController.GameTime + targetTime2.Seconds();
				data.TargetTime = ((timeSpan2 > data.TargetTime) ? timeSpan2 : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.FixTime:
			if (FixedTime > 0f)
			{
				CameraRig.Instance.CameraZoom.ZoomToTimed(tgt, out var targetTime, FixedTime, 0f, ZoomCurve);
				TimeSpan timeSpan = Game.Instance.Controllers.TimeController.GameTime + targetTime.Seconds();
				data.TargetTime = ((timeSpan > data.TargetTime) ? timeSpan : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.Snap:
			CameraRig.Instance.CameraZoom.ZoomToImmediate(tgt);
			return;
		}
		CameraRig.Instance.CameraZoom.CurrentNormalizePosition = tgt;
	}
}
