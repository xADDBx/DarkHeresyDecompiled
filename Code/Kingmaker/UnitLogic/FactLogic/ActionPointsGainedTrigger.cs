using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("36b9881761ae4529aa48aec231e3f060")]
public class ActionPointsGainedTrigger : ActionPointsChangedTrigger, IUnitGainActionPoints<EntitySubscriber>, IUnitGainActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitGainActionPoints, EntitySubscriber>, IUnitGainMovementPoints<EntitySubscriber>, IUnitGainMovementPoints, IEventTag<IUnitGainMovementPoints, EntitySubscriber>
{
	public void HandleUnitGainActionPoints(int actionPoints, IEvalContext context)
	{
		if (m_Type != 0 || !Restriction.IsPassed(context, base.Owner))
		{
			return;
		}
		using (ContextData<ActionPointsGainedGetter.ContextData>.Request().Setup(actionPoints))
		{
			base.Fact.RunActionInContext(Actions);
		}
	}

	public void HandleUnitGainMovementPoints(float movementPoints, IEvalContext context)
	{
		if (m_Type != PointsType.Blue || !Restriction.IsPassed(context, base.Owner))
		{
			return;
		}
		using (ContextData<ActionPointsGainedGetter.ContextData>.Request().Setup((int)movementPoints))
		{
			base.Fact.RunActionInContext(Actions);
		}
	}
}
