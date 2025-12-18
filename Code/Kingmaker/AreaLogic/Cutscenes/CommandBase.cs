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

	protected abstract void OnRun(CutscenePlayerData player, bool skipping);

	protected abstract void OnSetTime(double time, CutscenePlayerData player);

	protected abstract void OnSkip(CutscenePlayerData player);

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

	public virtual string GetWarning()
	{
		return null;
	}

	public virtual bool TrySkip(CutscenePlayerData player)
	{
		return !IsContinuous;
	}

	protected virtual void OnStop(CutscenePlayerData player)
	{
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

	public void Run(CutscenePlayerData player, bool skipping = false)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnRun(player, skipping);
			}
			catch (Exception e)
			{
				OnRunException();
				throw new FailedToRunCutsceneCommandException(player, this, e);
			}
		}
	}

	public void Skip(CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnSkip(player);
			}
			catch (Exception e)
			{
				OnRunException();
				throw new FailedToSkipCutsceneCommandException(player, this, e);
			}
		}
	}

	public void SetTime(double time, CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnSetTime(time, player);
			}
			catch (Exception e)
			{
				throw new FailedToTickCutsceneCommandException(player, this, e);
			}
		}
	}

	public void Stop(CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnStop(player);
			}
			catch (Exception e)
			{
				throw new FailedToStopCutsceneCommandException(player, this, e);
			}
		}
	}

	public virtual void Interrupt(CutscenePlayerData player)
	{
	}

	public virtual bool TryPrepareForStop(CutscenePlayerData player)
	{
		if (IsContinuous || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	protected virtual void OnRunException()
	{
	}
}
