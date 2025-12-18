using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using UnityEngine;

namespace Kingmaker.Tutorial;

public abstract class TutorialTriggerRulebookEvent<TRule> : TutorialTrigger, IGlobalRulebookHandler<TRule>, IRulebookHandler<TRule>, ISubscriber, IGlobalRulebookSubscriber where TRule : RulebookEvent
{
	private enum DirectlyControllableUnitRequirement
	{
		Initiator,
		Target
	}

	[SerializeField]
	private DirectlyControllableUnitRequirement m_DirectlyControllableRequirement;

	private bool ShouldTriggerInternal(TRule rule)
	{
		try
		{
			return ShouldTrigger(rule);
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
			return false;
		}
	}

	protected abstract bool ShouldTrigger(TRule rule);

	protected sealed override void SetupContext(TutorialContext context, RulebookEvent rule)
	{
		base.SetupContext(context, rule);
		try
		{
			OnSetupContext(context, (TRule)rule);
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
		}
	}

	protected virtual void OnSetupContext(TutorialContext context, TRule rule)
	{
	}

	public void OnEventAboutToTrigger(TRule rule)
	{
	}

	public void OnEventDidTrigger(TRule rule)
	{
		if (m_DirectlyControllableRequirement == DirectlyControllableUnitRequirement.Initiator && !rule.ConcreteInitiator.IsDirectlyControllable)
		{
			return;
		}
		if (m_DirectlyControllableRequirement == DirectlyControllableUnitRequirement.Target)
		{
			MechanicEntity target = rule.Target;
			if (target == null || !target.IsDirectlyControllable)
			{
				return;
			}
		}
		if (ShouldTriggerInternal(rule))
		{
			TryToTrigger(rule);
		}
	}
}
