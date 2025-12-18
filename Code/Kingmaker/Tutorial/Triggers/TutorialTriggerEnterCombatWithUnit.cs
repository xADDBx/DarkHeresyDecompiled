using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("a64f893727c8447bbe0fae2f6369b5b7")]
public abstract class TutorialTriggerEnterCombatWithUnit : TutorialTrigger, IPartyCombatHandler, ISubscriber
{
	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			return;
		}
		foreach (UnitGroupMemory.UnitInfo enemy in Game.Instance.Player.Group.Memory.Enemies)
		{
			BaseUnitEntity unit = enemy.Unit;
			if (IsSuitableUnit(unit))
			{
				TryToTrigger(null, delegate(TutorialContext context)
				{
					OnSetupContext(context, unit);
				});
				break;
			}
		}
	}

	protected abstract bool IsSuitableUnit(BaseUnitEntity unit);

	protected abstract void OnSetupContext(TutorialContext context, BaseUnitEntity unit);
}
