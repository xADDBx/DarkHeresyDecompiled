using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("33dbcae102934dd7809f7714f4980cd3")]
public class TutorialTriggerPreciseShotInterfaceToCoveredTarget : TutorialTrigger, IPreciseAttackUIHandler, ISubscriber
{
	public void HandleOpenPreciseAttackInterface(BaseUnitEntity target, bool targetCovered)
	{
		if (targetCovered)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.TargetUnit = target;
			});
		}
	}
}
