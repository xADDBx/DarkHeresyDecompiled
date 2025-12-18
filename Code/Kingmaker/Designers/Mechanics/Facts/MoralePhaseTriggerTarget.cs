using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[ComponentName("Morale/MoralePhaseTriggerTarget")]
[TypeId("3392892bf76749da8cf128fc8d4f6618")]
public sealed class MoralePhaseTriggerTarget : MoralePhaseTrigger, IMoralePhaseHandler<EntitySubscriber>, IMoralePhaseHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IMoralePhaseHandler, EntitySubscriber>
{
	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		TryTrigger(phase, EventInvokerExtensions.MechanicEntity);
	}
}
