using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.Mechanics;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("49b5845030444253a24c8293a0c16ddd")]
public abstract class MoraleChangeTrigger : MechanicEntityFactComponentDelegate
{
	public enum ChangeFilterType
	{
		None,
		GainMorale,
		LoseMorale
	}

	public MoraleEventType EventFilter = (MoraleEventType)(-1);

	public bool TriggerBeforeChange;

	public ChangeFilterType ChangeFilter;

	public bool CheckBaseValue;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnOwner = new ActionList();

	public ActionList ActionsOnCaster = new ActionList();

	public ActionList ActionsOnTarget = new ActionList();

	protected void TryTriggerBefore(RulePerformMoraleChange evt)
	{
		TryTrigger(evt, before: true);
	}

	protected void TryTriggerAfter(RulePerformMoraleChange evt)
	{
		TryTrigger(evt, before: false);
	}

	private void TryTrigger(RulePerformMoraleChange evt, bool before)
	{
		if (TriggerBeforeChange == before && IsEventSuitable(evt))
		{
			ActionsOnOwner.RunWithTarget(base.Owner);
			ActionsOnCaster.RunWithTarget(evt.Initiator);
			ActionsOnTarget.RunWithTarget(evt.Target);
		}
	}

	private bool IsEventSuitable(RulePerformMoraleChange evt)
	{
		bool flag = evt.EventType.HasAnyFlag(EventFilter);
		if (flag)
		{
			flag = ChangeFilter switch
			{
				ChangeFilterType.None => true, 
				ChangeFilterType.GainMorale => evt.ResultDeltaRaw > 0 || (CheckBaseValue && evt.BaseValue > 0), 
				ChangeFilterType.LoseMorale => evt.ResultDeltaRaw < 0 || (CheckBaseValue && evt.BaseValue < 0), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (flag)
		{
			return Restrictions.IsPassed(base.Context, null, evt.Target, evt);
		}
		return false;
	}
}
