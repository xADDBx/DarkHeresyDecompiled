using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandUnitLookAt")]
[TypeId("ccbd07b7ffc946647a6a30d5628d1836")]
public class CommandUnitLookAt : CommandBase
{
	private class Data
	{
		public float InitialOrientation;

		public float AngularSpeed;

		public Vector3? TargetPosition;

		public bool TurnFullBody;

		public bool Finished;

		public double LastTime;

		public double? TimeToReset;

		public bool IsAlreadyReset;

		public bool Signalled;

		public bool Freezed;
	}

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private PositionEvaluator m_Position;

	[SerializeField]
	private Vector3 m_Offset = new Vector3(0f, 1.7f, 0f);

	[SerializeField]
	[Tooltip("Angular speed in degrees per second\nIf set to 0, used Unit's default angular speed")]
	private float m_AngularSpeed;

	[SerializeField]
	[Tooltip("If true, Unit will turn to target position\nIf false, Unit will try to turn only head and upper body to look at target position. If it's not possible, Unit will turn full body")]
	private bool m_TurnFullBody = true;

	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[ShowIf("IsContinuous")]
	[Tooltip("If true, Unit will restore its orientation after command stops")]
	private bool m_RestoreOrientation;

	[SerializeField]
	[ShowIf("IsContinuous")]
	[Tooltip("If true, Unit will freeze in direction after turn completed even if target position changed\nIf false, Unit will look after target position even if it will be changed")]
	private bool m_FreezeAfterTurn = true;

	[SerializeField]
	[ShowIf("HasDuration")]
	[Tooltip("Unit will stop to look at target position after this Duration in seconds")]
	private float m_Duration;

	[SerializeField]
	private CommandSignalData m_OnTurned = new CommandSignalData
	{
		Name = "OnTurned"
	};

	private bool HasDuration
	{
		get
		{
			if (!IsContinuous)
			{
				return !m_TurnFullBody;
			}
			return false;
		}
	}

	private float Duration
	{
		get
		{
			if (!HasDuration)
			{
				return 0f;
			}
			return Mathf.Max(m_Duration, 0.1f);
		}
	}

	public override bool IsContinuous => m_Continuous;

	private void TurnImmediately(CutscenePlayerData player)
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		if (!(value is LightweightUnitEntity) && (value == null || !value.CanRotate))
		{
			player.GetCommandData<Data>(this).Finished = !m_Continuous;
		}
		else if (m_TurnFullBody)
		{
			value.SetOrientation(value.GetOrientationTo(m_Position.GetValue()));
		}
		else if (!HasDuration)
		{
			value.LookAt(m_Position.GetValue(), 0.1f);
		}
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity value = m_Unit.GetValue();
		Vector3 point = m_Position.GetValue() + m_Offset;
		float orientationTo = value.GetOrientationTo(point);
		commandData.InitialOrientation = value.DesiredOrientation;
		commandData.AngularSpeed = ((m_AngularSpeed > 0f) ? m_AngularSpeed : value.MovementAgent.AngularSpeedWhenStand);
		commandData.TurnFullBody = m_TurnFullBody || UnitLookAtIKExtensions.GetDeltaAngle(commandData.InitialOrientation, orientationTo) > 80f;
		if (skipping)
		{
			TurnImmediately(player);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity value = m_Unit.GetValue();
		if (m_RestoreOrientation || !m_TurnFullBody)
		{
			value.StopLookAt();
			value.DesiredOrientation = commandData.InitialOrientation;
		}
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		TurnImmediately(player);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		Vector3 vector = m_Position.GetValue() + m_Offset;
		float orientationTo = value.GetOrientationTo(vector);
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.Freezed || HasDuration)
		{
			TickTurning(time, commandData, value, vector, orientationTo);
		}
		if (!IsContinuous && (value == null || !value.IsInGame || !value.CanRotate))
		{
			commandData.Finished = true;
			return;
		}
		if (commandData.Freezed || IsTurningCompleted(value, vector, orientationTo))
		{
			if (!commandData.TimeToReset.HasValue && HasDuration)
			{
				commandData.TimeToReset = time + (double)Duration;
			}
			if (!m_Continuous)
			{
				commandData.Finished = !HasDuration || commandData.IsAlreadyReset;
			}
			if (m_FreezeAfterTurn)
			{
				commandData.Freezed = true;
			}
			if (!string.IsNullOrEmpty(m_OnTurned.GateId) && !commandData.Signalled)
			{
				commandData.Signalled = true;
				player.SignalGateExtra(m_OnTurned.GateId);
			}
		}
		if (time > 20.0)
		{
			player.GetCommandData<Data>(this).Finished = true;
		}
	}

	private void TickTurning(double time, Data data, AbstractUnitEntity unit, Vector3 targetPosition, float targetAngle)
	{
		if (!data.IsAlreadyReset)
		{
			if (data.TurnFullBody)
			{
				TickTurningFullBody(time, data, unit, targetAngle);
			}
			else
			{
				TickTurningUpperBody(time, data, unit, targetPosition, targetAngle);
			}
		}
	}

	private void TickTurningFullBody(double time, Data data, AbstractUnitEntity unit, float targetAngle)
	{
		bool num = IsTimeToReset(time, data);
		targetAngle = (num ? data.InitialOrientation : targetAngle);
		float num2 = Mathf.MoveTowardsAngle(maxDelta: (float)(time - data.LastTime) * data.AngularSpeed, current: unit.Orientation, target: targetAngle);
		unit.SetOrientation(num2);
		data.LastTime = time;
		if (num && Mathf.Approximately(num2, targetAngle))
		{
			data.IsAlreadyReset = true;
		}
	}

	private void TickTurningUpperBody(double time, Data data, AbstractUnitEntity unit, Vector3 targetPosition, float targetAngle)
	{
		UnitLookAtIKExtensions.GetDeltaAngle(data.InitialOrientation, targetAngle);
		if (IsTimeToReset(time, data))
		{
			float turningTime = (data.TargetPosition.HasValue ? (unit.GetDeltaAngle(data.TargetPosition.Value) / data.AngularSpeed) : 0.3f);
			unit.StopLookAt(turningTime);
			data.IsAlreadyReset = true;
			return;
		}
		Vector3? targetPosition2 = data.TargetPosition;
		if (targetPosition != targetPosition2 && (!data.TargetPosition.HasValue || unit.IsLookingAt(data.TargetPosition.Value, targetAngle)))
		{
			float turningTime2 = UnitLookAtIKExtensions.GetDeltaAngle(data.TargetPosition.HasValue ? unit.GetOrientationTo(data.TargetPosition.Value) : unit.Orientation, targetAngle) / data.AngularSpeed;
			data.TargetPosition = targetPosition;
			unit.LookAt(targetPosition, turningTime2);
		}
	}

	private bool IsTurningCompleted(AbstractUnitEntity unit, Vector3 targetPosition, float targetAngle)
	{
		if (m_TurnFullBody)
		{
			return Mathf.Approximately(unit.Orientation, targetAngle);
		}
		return unit.IsLookingAt(targetPosition);
	}

	private bool IsTimeToReset(double time, Data data)
	{
		if (HasDuration && data.TimeToReset.HasValue)
		{
			return time > data.TimeToReset;
		}
		return false;
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (IsContinuous)
		{
			return new CommandSignalData[1] { m_OnTurned };
		}
		return base.GetExtraSignals();
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		TurnImmediately(player);
	}

	public override string GetCaption()
	{
		return m_Unit?.GetCaptionShort() + " <b>look at</b> " + m_Position?.GetCaptionShort();
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!m_Unit || !m_Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
