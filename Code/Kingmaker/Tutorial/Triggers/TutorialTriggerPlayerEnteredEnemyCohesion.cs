using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Cohesion;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("5a5b3b8ca756ba94a95533d33ef7be59")]
public class TutorialTriggerPlayerEnteredEnemyCohesion : TutorialTrigger, IEntityEnterCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber
{
	private bool m_IsTriggered;

	void IEntityEnterCohesionRangeHandler.HandleEntityEnterCohesionRange(MechanicEntity entity)
	{
		if (m_IsTriggered)
		{
			return;
		}
		BaseUnitEntity unit = entity as BaseUnitEntity;
		if (unit == null || !unit.IsPlayerFaction || !unit.IsInCombat)
		{
			return;
		}
		BaseUnitEntity cohesionOwner = TutorialEnemyCohesionQuery.FindEnemyOwner((PartCohesion c) => c.ContainsInRange(unit));
		if (cohesionOwner != null)
		{
			TryToTrigger(null, delegate(TutorialContext ctx)
			{
				ctx.SourceUnit = unit;
				ctx.TargetUnit = cohesionOwner;
			});
			m_IsTriggered = true;
		}
	}
}
