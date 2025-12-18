using System;
using Kingmaker.AreaLogic.Cutscenes.Commands;
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
			RunInternal(skipping);
		}
		catch (Exception ex)
		{
			HandleCommandException(ex);
		}
	}

	public void Tick(bool skipping)
	{
		try
		{
			TickInternal(skipping);
		}
		catch (Exception ex)
		{
			HandleCommandException(ex);
		}
	}

	public bool Interrupt<T>()
	{
		try
		{
			return InterruptInternal<T>();
		}
		catch (Exception ex)
		{
			HandleCommandException(ex);
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
			Complete();
		}
		catch (Exception ex)
		{
			HandleCommandException(ex);
		}
	}

	public void ForceStop(bool releaseControlledUnit)
	{
		try
		{
			ForceStopInternal(releaseControlledUnit);
		}
		catch (Exception ex)
		{
			HandleCommandException(ex);
		}
	}

	public void Resume()
	{
		if (!m_Player.Paused)
		{
			Run(skipping: false);
		}
	}

	private void RunInternal(bool skipping)
	{
		MarkUnit();
		if (skipping && m_Command.TrySkip(m_Player))
		{
			m_Command.Skip(m_Player);
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
				m_Command.Run(m_Player, skipping);
			}
		}
		if (m_Command.IsFinished(m_Player) && !m_Command.IsContinuous)
		{
			Complete();
		}
	}

	private void TickInternal(bool skipping)
	{
		float deltaTime = Game.Instance.Controllers.TimeController.DeltaTime;
		PlayTime += deltaTime;
		m_DebugData.PlayTime = PlayTime;
		m_Command.SetTime(PlayTime, m_Player);
		if (skipping && m_Command.TrySkip(m_Player))
		{
			m_Command.Interrupt(m_Player);
			m_DebugData.IsSkipped = true;
		}
		if (m_Command.TryPrepareForStop(m_Player))
		{
			Complete();
		}
	}

	private void Complete()
	{
		m_Command.Stop(m_Player);
		m_DebugData.IsComplete = true;
		IsComplete = true;
		ReleaseUnit();
	}

	private bool InterruptInternal<T>()
	{
		if (!(m_Command is T))
		{
			return false;
		}
		m_Command.Interrupt(m_Player);
		return true;
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

	private void HandleCommandException(Exception ex)
	{
		IsComplete = true;
		ReleaseUnit();
		m_DebugData.IsFailed = true;
		m_DebugData.ExceptionMessage = ex.Message;
		m_Player.FailedCommands.Add(m_Command);
		m_Player.HandleException(ex, m_Track, m_Command);
	}

	public bool MarkUnit()
	{
		IAbstractUnitEntity controlledUnit = m_Command.GetControlledUnit();
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
		IAbstractUnitEntity controlledUnit = m_Command.GetControlledUnit();
		if (controlledUnit != null)
		{
			CutsceneControlledUnit.ReleaseUnit(controlledUnit, m_Player);
			m_DebugData.IsUnitUnderControl = false;
		}
	}

	public IAbstractUnitEntity GetControlledUnit()
	{
		return m_Command.GetControlledUnit();
	}
}
