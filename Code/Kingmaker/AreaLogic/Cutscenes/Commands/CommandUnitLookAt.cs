using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
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

		public bool Finished;

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
	[Tooltip("Angular speed in degrees per second\nIf set to 0, used Unit's default angular speed")]
	private float m_AngularSpeed;

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
	private CommandSignalData m_OnTurned = new CommandSignalData
	{
		Name = "OnTurned"
	};

	public override bool IsContinuous => m_Continuous;

	public override bool ShouldHaveControlledUnit => true;

	private CommandResult TurnImmediately(CutscenePlayerData player)
	{
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		value.MovementAgent.OverridenAngularSpeed = null;
		if (!(value is LightweightUnitEntity) && (value == null || !value.CanRotate))
		{
			player.GetCommandData<Data>(this).Finished = !m_Continuous;
			return CommandResult.Success;
		}
		value.SetOrientation(value.GetOrientationTo(m_Position.GetValue()));
		player.GetCommandData<Data>(this).Finished = true;
		return CommandResult.Success;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		commandData.InitialOrientation = value.DesiredOrientation;
		value.MovementAgent.OverridenAngularSpeed = ((m_AngularSpeed > 0f) ? m_AngularSpeed : value.MovementAgent.CurrentAngularSpeed);
		if (!skipping)
		{
			return CommandResult.Success;
		}
		return TurnImmediately(player);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		value.MovementAgent.OverridenAngularSpeed = null;
		if (m_RestoreOrientation)
		{
			Data commandData = player.GetCommandData<Data>(this);
			value.DesiredOrientation = commandData.InitialOrientation;
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return TurnImmediately(player);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		if (!m_Position.TryGetValue(out var value2))
		{
			return CommandResult.Fail("Failed to find target position");
		}
		float orientationTo = value.GetOrientationTo(value2);
		Data commandData = player.GetCommandData<Data>(this);
		bool flag = commandData.Freezed || IsTurningCompleted(value, orientationTo);
		if (!flag)
		{
			TickTurning(value, orientationTo);
		}
		if (!IsContinuous && (value == null || !value.IsInGame || !value.CanRotate))
		{
			commandData.Finished = true;
			return CommandResult.Success;
		}
		if (flag)
		{
			if (!m_Continuous)
			{
				commandData.Finished = true;
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
		return CommandResult.Success;
	}

	private static void TickTurning(AbstractUnitEntity unit, float targetAngle)
	{
		unit.DesiredOrientation = targetAngle;
	}

	private static bool IsTurningCompleted(AbstractUnitEntity unit, float targetAngle)
	{
		return Mathf.Approximately(unit.Orientation, targetAngle);
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (IsContinuous)
		{
			return new CommandSignalData[1] { m_OnTurned };
		}
		return base.GetExtraSignals();
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return TurnImmediately(player);
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
