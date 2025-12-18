using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0f0841e92e334b1ab0f2205d7462f313")]
public class PushCollisionTrigger : UnitBuffComponentDelegate, IGlobalRulebookHandler<RulePerformCollision>, IRulebookHandler<RulePerformCollision>, ISubscriber, IGlobalRulebookSubscriber
{
	public ContextValue ValueMultiplier;

	public ContextValue ValueBonus;

	public ContextPropertyName ContextPropertyName;

	public bool OnlyFromOwner;

	public ActionList Actions;

	public void OnEventAboutToTrigger(RulePerformCollision evt)
	{
	}

	public void OnEventDidTrigger(RulePerformCollision evt)
	{
		if (OnlyFromOwner && evt.Pusher != base.Owner)
		{
			return;
		}
		base.Context[ContextPropertyName] = evt.ResultDamage * ValueMultiplier.Calculate(base.Context) + ValueBonus.Calculate(base.Context);
		using (base.Fact.MaybeContext?.SetScope(base.Owner))
		{
			base.Fact.RunActionInContext(Actions, evt.Pushed);
		}
	}
}
