using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("5a79661deb524ee092d0a54f0ced96ff")]
public class TutorialTriggerPreciseShotInterfaceToBuffedTarget : TutorialTrigger, IPreciseAttackUIHandler, ISubscriber
{
	public BlueprintBuffReference Buff;

	public bool Revert;

	public void HandleOpenPreciseAttackInterface(BaseUnitEntity target, bool targetCovered)
	{
		if (targetCovered)
		{
			return;
		}
		if (Revert)
		{
			if (target.Buffs.GetBuff(Buff) == null)
			{
				TryToTrigger(null, delegate(TutorialContext context)
				{
					context.TargetUnit = target;
				});
			}
		}
		else if (target.Buffs.GetBuff(Buff) != null)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.TargetUnit = target;
			});
		}
	}
}
