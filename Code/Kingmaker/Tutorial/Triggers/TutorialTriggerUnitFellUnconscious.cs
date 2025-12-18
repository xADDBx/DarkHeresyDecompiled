using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("d76775862c1a4e94bc9867c46fd70fe7")]
public class TutorialTriggerUnitFellUnconscious : TutorialTrigger, IUnitDeathHandler, ISubscriber
{
	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.SpaceCombat))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = unitEntity as BaseUnitEntity;
		if (baseUnitEntity != null && unitEntity.IsPlayerFaction)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.TargetUnit = baseUnitEntity;
			});
		}
	}
}
