using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class MoraleAsksController : BaseAsksController, IMoralePhaseHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public void HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		if (!(EventInvokerExtensions.MechanicEntity is AbstractUnitEntity { CanAct: not false } abstractUnitEntity))
		{
			return;
		}
		using (EvalContext.PushAsksContext(abstractUnitEntity, abstractUnitEntity))
		{
			switch (phase)
			{
			case MoralePhaseType.Broken:
				abstractUnitEntity.View.Asks?.BrokenMorale.Schedule();
				break;
			case MoralePhaseType.Heroic:
				abstractUnitEntity.View.Asks?.HeroicMorale.Schedule();
				break;
			}
		}
	}
}
