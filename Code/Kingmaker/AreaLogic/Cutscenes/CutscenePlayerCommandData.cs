using System;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutscenePlayerCommandData
{
	public class DebugCommandPlayerData
	{
		public string CommandId;

		public bool IsActive;

		public bool IsComplete;

		public bool IsFailed;

		public bool IsSkipped;

		public bool IsForceStopped;

		public double PlayTime;

		public bool? PassedCondition;

		public string ExceptionMessage;

		public bool? IsUnitUnderControl;

		public string MarkedUnit;

		public bool IsDisabledDueToError;

		public void Reset()
		{
			IsActive = false;
			IsComplete = false;
			IsSkipped = false;
			IsForceStopped = false;
			PlayTime = 0.0;
			PassedCondition = null;
		}
	}

	private readonly CutscenePlayerTrackData m_Track;

	private readonly CutscenePlayerData m_Player;

	private readonly CommandBase m_Command;

	private DebugCommandPlayerData m_DebugData = new DebugCommandPlayerData();

	public bool IsDisabledDueToError { get; private set; }

	public bool IsComplete { get; private set; }

	public float PlayTime { get; private set; }

	public DebugCommandPlayerData DebugData => m_DebugData;

	public CutscenePlayerCommandData(CommandReference reference, CutscenePlayerTrackData track, CutscenePlayerData player)
	{
		m_Track = track;
		m_Player = player;
		m_Command = reference.Get();
		m_DebugData.CommandId = m_Command.AssetGuid;
	}

	public void Restart()
	{
		IsComplete = false;
		PlayTime = 0f;
		m_DebugData.Reset();
	}

	public void SetInitialDebugData()
	{
		IAbstractUnitEntity controlledUnit = GetControlledUnit();
		if (controlledUnit != null)
		{
			m_DebugData.MarkedUnit = controlledUnit.ToString();
			m_DebugData.IsUnitUnderControl = false;
		}
		else
		{
			m_DebugData.IsUnitUnderControl = null;
		}
	}

	public (bool canRun, CommandBase.EntryFailResult OnFail) CheckEntryCondition()
	{
		bool flag = m_Command.EntryCondition.Check();
		m_DebugData.PassedCondition = flag;
		if (!flag)
		{
			m_Player.FailedCheckCommands.Add(m_Command);
		}
		return (canRun: flag, OnFail: m_Command.OnFail);
	}

	public void Run(bool skipping)
	{
		try
		{
			HandleCommandResult(RunInternal(skipping));
		}
		catch (Exception e)
		{
			HandleCommandResult(CommandBase.CommandResult.FromException(e));
		}
	}

	public void Tick(bool skipping)
	{
		try
		{
			HandleCommandResult(TickInternal(skipping));
		}
		catch (Exception e)
		{
			HandleCommandResult(CommandBase.CommandResult.FromException(e));
		}
	}

	public bool Interrupt<T>()
	{
		try
		{
			if (!(m_Command is T))
			{
				return false;
			}
			CommandBase.CommandResult result = m_Command.Interrupt(m_Player);
			HandleCommandResult(result);
			return result.IsSuccess;
		}
		catch (Exception e)
		{
			HandleCommandResult(CommandBase.CommandResult.FromException(e));
		}
		return false;
	}

	public void CompleteContinuous()
	{
		if (IsComplete)
		{
			return;
		}
		try
		{
			HandleCommandResult(Complete());
		}
		catch (Exception e)
		{
			HandleCommandResult(CommandBase.CommandResult.FromException(e));
		}
	}

	public void ForceStop(bool releaseControlledUnit)
	{
		try
		{
			ForceStopInternal(releaseControlledUnit);
		}
		catch (Exception e)
		{
			HandleCommandResult(CommandBase.CommandResult.FromException(e));
		}
	}

	public void Resume()
	{
		if (!m_Player.Paused)
		{
			Run(skipping: false);
		}
	}

	private CommandBase.CommandResult RunInternal(bool skipping)
	{
		CommandBase.CommandResult result = CommandBase.CommandResult.Success;
		MarkUnit();
		if (skipping && m_Command.TrySkip(m_Player))
		{
			result = m_Command.Skip(m_Player);
			m_DebugData.IsSkipped = true;
		}
		else
		{
			using (ProfileScope.New("Run Command"))
			{
				m_Player.ClearCommandData(m_Command);
				m_DebugData.PlayTime = 0.0;
				m_DebugData.IsActive = true;
				PlayTime = 0f;
				result = m_Command.Run(m_Player, skipping);
			}
		}
		if (m_Command.IsFinished(m_Player) && !m_Command.IsContinuous)
		{
			result = Complete();
		}
		return result;
	}

	private CommandBase.CommandResult TickInternal(bool skipping)
	{
		float deltaTime = Game.Instance.Controllers.TimeController.DeltaTime;
		PlayTime += deltaTime;
		m_DebugData.PlayTime = PlayTime;
		CommandBase.CommandResult result = m_Command.SetTime(PlayTime, m_Player);
		if (!result.IsSuccess)
		{
			return result;
		}
		if (skipping && m_Command.TrySkip(m_Player))
		{
			result = m_Command.Interrupt(m_Player);
			m_DebugData.IsSkipped = true;
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		if (m_Command.TryPrepareForStop(m_Player))
		{
			result = Complete();
		}
		return result;
	}

	private CommandBase.CommandResult Complete()
	{
		CommandBase.CommandResult result = m_Command.Stop(m_Player);
		m_DebugData.IsComplete = true;
		IsComplete = true;
		ReleaseUnit();
		return result;
	}

	private void ForceStopInternal(bool releaseControlledUnit)
	{
		if (!m_Command.IsFinished(m_Player))
		{
			m_Command.Stop(m_Player);
		}
		if (releaseControlledUnit)
		{
			ReleaseUnit();
		}
	}

	private void HandleCommandResult(CommandBase.CommandResult result)
	{
		if (result.IsSuccess)
		{
			return;
		}
		IsComplete = true;
		ReleaseUnit();
		m_DebugData.IsFailed = true;
		m_DebugData.ExceptionMessage = "cmd " + m_Command.NameSafe() + ": " + result.ErrorMessage;
		if (m_Command.EvaluationErrorHandlingPolicy == EvaluationErrorHandlingPolicy.Ignore)
		{
			return;
		}
		if (m_Player.FailedCommands.Contains(m_Command))
		{
			IsDisabledDueToError = true;
			m_DebugData.IsDisabledDueToError = true;
			return;
		}
		m_Player.LogError("[" + m_Player.Cutscene.Name + "] error in cmd " + m_Command.NameSafe() + ": " + result.ErrorMessage, result.LoudReport, m_Command);
		switch (m_Command.EvaluationErrorHandlingPolicy)
		{
		case EvaluationErrorHandlingPolicy.SkipTrack:
			m_Track.ForceStop(shouldSignal: true, releaseControlledUnit: true);
			break;
		case EvaluationErrorHandlingPolicy.SkipGate:
			m_Player.StopGateOnErrorInsideTrack(m_Track);
			break;
		}
	}

	public bool MarkUnit()
	{
		IAbstractUnitEntity controlledUnit = GetControlledUnit();
		if (controlledUnit != null)
		{
			bool num = CutsceneControlledUnit.MarkUnit(controlledUnit, m_Player);
			if (num)
			{
				m_DebugData.IsUnitUnderControl = true;
				return num;
			}
			m_DebugData.IsUnitUnderControl = false;
			m_DebugData.ExceptionMessage = $"Failed to set mark on unit {controlledUnit}";
			return num;
		}
		return true;
	}

	private void ReleaseUnit()
	{
		IAbstractUnitEntity controlledUnit = GetControlledUnit();
		if (controlledUnit != null)
		{
			CutsceneControlledUnit.ReleaseUnit(controlledUnit, m_Player);
			m_DebugData.IsUnitUnderControl = false;
		}
	}

	public IAbstractUnitEntity GetControlledUnit()
	{
		try
		{
			return m_Command.GetControlledUnit();
		}
		catch (Exception ex)
		{
			m_DebugData.IsFailed = true;
			m_DebugData.ExceptionMessage = ex.Message;
			m_Player.LogError("[" + m_Player.Cutscene.Name + "] error in cmd " + m_Command.NameSafe() + " failed to get controlled unit", needQaReport: true, m_Command);
			return null;
		}
	}

	public void ClearErrorStatus()
	{
		IsDisabledDueToError = false;
		DebugData.IsDisabledDueToError = false;
	}
}
