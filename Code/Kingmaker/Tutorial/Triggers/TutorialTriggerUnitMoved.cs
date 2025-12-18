using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("cad79c5fc5de4d089d79505ea06b6d59")]
public class TutorialTriggerUnitMoved : TutorialTrigger, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	private bool m_IsTriggered;

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (!unit.IsInCombat || !unit.IsPlayerFaction || m_IsTriggered)
		{
			return;
		}
		BaseUnitEntity baseUnit = unit as BaseUnitEntity;
		if (baseUnit != null)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = baseUnit;
			});
			m_IsTriggered = true;
		}
	}
}
