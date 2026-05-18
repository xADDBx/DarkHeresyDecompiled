using System.Text;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandUnitTurnHead")]
[TypeId("e9f35725de924224b884e9fa1f57f158")]
public class CommandUnitTurnHead : CommandBase
{
	private enum TurningMode
	{
		TurnToTarget,
		TurnAngle
	}

	private class Data
	{
		public bool IsStarted;

		public bool IsTurnCompleted;

		public bool Signalled;

		public bool Freezed;

		public IVector3PositionProvider PositionProvider;

		private bool m_Finished;

		private IAbstractUnitEntity m_Unit;

		public bool Finished
		{
			get
			{
				return m_Finished;
			}
			set
			{
				m_Finished = value;
				if (m_Finished)
				{
					m_Unit?.StopLookAt();
				}
			}
		}

		public void SetUp(IAbstractUnitEntity unit)
		{
			m_Unit = unit;
		}
	}

	private class EvaluatorPositionProvider : IVector3PositionProvider
	{
		private readonly PositionEvaluator m_Evaluator;

		private readonly Vector3 m_Offset;

		public Vector3 Position => m_Evaluator.GetValue() + m_Offset;

		public EvaluatorPositionProvider(PositionEvaluator evaluator, Vector3 offset)
		{
			m_Evaluator = evaluator;
			m_Offset = offset;
		}
	}

	[SerializeField]
	private TurningMode m_TurningMode;

	[SerializeField]
	private RotatedBonesSet m_RotatedBones;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	[ShowIf("IsTurningToTarget")]
	private PositionEvaluator m_Position;

	[SerializeField]
	[ShowIf("IsTurningToTarget")]
	private Vector3 m_Offset = new Vector3(0f, 1.7f, 0f);

	[SerializeField]
	[HideIf("IsTurningToTarget")]
	[Range(-80f, 80f)]
	private int m_Angle;

	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[ShowIf("HasDuration")]
	[Tooltip("Unit will stop to look at target position after [Duration] seconds")]
	[Range(0f, 60f)]
	private float m_Duration;

	[SerializeField]
	[Tooltip("Unit will turn head to target in [TurningDuration] seconds")]
	[Range(0.1f, 10f)]
	private float m_TurningDuration = 0.7f;

	[SerializeField]
	[Tooltip("If true, Unit will freeze in direction after turn completed even if target position changed\nIf false, Unit will look after target position even if it will be changed")]
	private bool m_FreezeAfterTurn = true;

	[SerializeField]
	private CommandSignalData m_OnTurned = new CommandSignalData
	{
		Name = "OnTurned"
	};

	public override bool IsContinuous => m_Continuous;

	public override bool ShouldHaveControlledUnit => true;

	private bool HasDuration => !IsContinuous;

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

	private bool IsTurningToTarget => m_TurningMode == TurningMode.TurnToTarget;

	public override string GetCaption()
	{
		StringBuilder stringBuilder = new StringBuilder(m_Unit?.GetCaptionShort());
		stringBuilder.Append((m_RotatedBones == RotatedBonesSet.HeadAndSpine) ? " <b>turn head and spine</b> " : " <b>turn head</b> ");
		stringBuilder.Append((m_TurningMode == TurningMode.TurnToTarget) ? ("to " + m_Position?.GetCaptionShort()) : $"on angle {m_Angle}");
		return stringBuilder.ToString();
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!m_Unit || !m_Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		commandData.SetUp(value);
		CommandResult result = TryCreatePositionProvider(value, out commandData.PositionProvider);
		if (!result.IsSuccess)
		{
			return result;
		}
		if (!skipping)
		{
			return CommandResult.Success;
		}
		return Finish(player);
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.IsStarted)
		{
			value.LookAtWithoutTurnTo(commandData.PositionProvider, m_RotatedBones, m_TurningDuration, Duration);
			commandData.IsStarted = true;
		}
		bool flag = value.IsFinishedLookAtCommands();
		commandData.IsTurnCompleted |= commandData.Freezed || flag || value.IsLookingAt(commandData.PositionProvider.Position);
		if (!IsContinuous && (value == null || !value.IsInGame || !value.CanRotate))
		{
			return Finish(player);
		}
		if (commandData.IsTurnCompleted)
		{
			if (!m_Continuous && flag)
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
			return Finish(player);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return Finish(player);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return Finish(player);
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return Finish(player);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (IsContinuous)
		{
			return new CommandSignalData[1] { m_OnTurned };
		}
		return base.GetExtraSignals();
	}

	private CommandResult Finish(CutscenePlayerData player)
	{
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		value.StopLookAt(m_TurningDuration);
		player.GetCommandData<Data>(this).Finished = true;
		return CommandResult.Success;
	}

	private CommandResult TryCreatePositionProvider(IAbstractUnitEntity unit, out IVector3PositionProvider positionProvider)
	{
		positionProvider = null;
		if (m_TurningMode == TurningMode.TurnToTarget)
		{
			if (!m_Position.TryGetValue(out var _))
			{
				return CommandResult.Fail("Failed to find target position");
			}
			positionProvider = new EvaluatorPositionProvider(m_Position, m_Offset);
			return CommandResult.Success;
		}
		if (m_TurningMode == TurningMode.TurnAngle)
		{
			Quaternion quaternion = Quaternion.Euler(0f, m_Angle, 0f);
			Vector3 position = unit.Position + LookAtIKController.EyeShift + quaternion * unit.Forward;
			positionProvider = new ConstantPositionProvider(position);
			return CommandResult.Success;
		}
		return CommandResult.Fail($"Turning mode {m_TurningMode} is not supported!");
	}
}
