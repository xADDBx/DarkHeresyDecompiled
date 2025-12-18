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
[ComponentName("Morale/MoralePhaseTriggerGlobal")]
[TypeId("aac6aab7b91d45dba15c958ffe4f586b")]
public sealed class MoralePhaseTriggerGlobal : MoralePhaseTrigger, IMoralePhaseHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		TryTrigger(phase, EventInvokerExtensions.MechanicEntity);
	}
}
