using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Morale/MoraleAddingAtTurnStart")]
[TypeId("2a8b847c501f43bb8f6ab6594d13f38e")]
public class MoraleAddingAtTurnStart : UnitFactComponentDelegate, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>
{
	public RestrictionCalculator Restrictions;

	public ContextValue Amount;

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased && Restrictions.IsPassed(base.Context, base.Owner))
		{
			Rulebook.Trigger(new RulePerformMoraleChange(base.Owner, base.Owner, MoraleEventType.TurnStart, Amount.Calculate(base.Context)));
		}
	}
}
