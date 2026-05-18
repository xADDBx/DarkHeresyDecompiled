using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("8b0d4441623a3634586b16435a8da106")]
public abstract class CommandBase : ElementsScriptableObject, IEvaluationErrorHandlingPolicyHolder
{
	public enum EntryFailResult
	{
		RemoveTrack,
		FinishTrack,
		SkipCommand
	}

	[Serializable]
	public class CommandSignalData
	{
		[InspectorReadOnly]
		public string Name;

		public string GateId;
	}

	public struct CommandResult
	{
		public bool IsSuccess;

		public string ErrorMessage;

		public bool LoudReport;

		public static CommandResult Success
		{
			get
			{
				CommandResult result = default(CommandResult);
				result.IsSuccess = true;
				return result;
			}
		}

		public static CommandResult Fail(string message)
		{
			CommandResult result = default(CommandResult);
			result.IsSuccess = false;
			result.ErrorMessage = message;
			return result;
		}

		public static CommandResult FailWithReport(string message)
		{
			CommandResult result = default(CommandResult);
			result.IsSuccess = false;
			result.ErrorMessage = message;
			result.LoudReport = true;
			return result;
		}

		public static CommandResult FromException(Exception e)
		{
			CommandResult result = default(CommandResult);
			result.IsSuccess = false;
			result.ErrorMessage = e.Message;
			return result;
		}
	}

	protected const int TakingToLongDefaultSeconds = 20;

	public ConditionsChecker EntryCondition;

	[ShowIf("HasConditions")]
	public EntryFailResult OnFail;

	[SerializeField]
	private EvaluationErrorHandlingPolicy m_EvaluationErrorHandlingPolicy;

	[SerializeField]
	private bool m_IsDisabled;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy => m_EvaluationErrorHandlingPolicy;

	public bool IsDisabled => m_IsDisabled;

	public bool HasConditions
	{
		get
		{
			if (EntryCondition != null)
			{
				return EntryCondition.HasConditions;
			}
			return false;
		}
	}

	public virtual bool IsContinuous => false;

	public virtual bool ShouldHaveControlledUnit => false;

	protected abstract CommandResult OnRun(CutscenePlayerData player, bool skipping);

	protected abstract CommandResult OnSetTime(double time, CutscenePlayerData player);

	protected abstract CommandResult OnSkip(CutscenePlayerData player);

	protected abstract CommandResult OnStop(CutscenePlayerData player);

	public abstract CommandResult Interrupt(CutscenePlayerData player);

	public abstract bool IsFinished(CutscenePlayerData player);

	public virtual string GetCaption()
	{
		return GetType().Name;
	}

	public virtual string GetDescription()
	{
		return "Empty description";
	}

	public virtual CommandSignalData[] GetExtraSignals()
	{
		return null;
	}

	[CanBeNull]
	public virtual string GetWarning()
	{
		return null;
	}

	public virtual bool TrySkip(CutscenePlayerData player)
	{
		return !IsContinuous;
	}

	protected virtual bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		return true;
	}

	[CanBeNull]
	public virtual IAbstractUnitEntity GetControlledUnit()
	{
		return null;
	}

	public CommandResult Run(CutscenePlayerData player, bool skipping = false)
	{
		using (player.GetDataScope())
		{
			return OnRun(player, skipping);
		}
	}

	public CommandResult Skip(CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			return OnSkip(player);
		}
	}

	public CommandResult SetTime(double time, CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			return OnSetTime(time, player);
		}
	}

	public CommandResult Stop(CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			return OnStop(player);
		}
	}

	public virtual bool TryPrepareForStop(CutscenePlayerData player)
	{
		if (IsContinuous || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}
}
