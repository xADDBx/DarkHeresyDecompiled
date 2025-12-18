using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Concentration.Events;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Features.Concentration;

[Serializable]
[TypeId("54892dc02f5c4f89affa83e0ba310699")]
[ClassInfoBox("Срабатывает когда Owner'у сбили концентрацию")]
[ComponentName("Concentration/ConcentrationBrokenTriggerOwner")]
public sealed class ConcentrationBrokenTriggerOwner : ConcentrationBrokenTrigger, IConcentrationBrokenHandler<EntitySubscriber>, IConcentrationBrokenHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IConcentrationBrokenHandler, EntitySubscriber>
{
	void IConcentrationBrokenHandler.HandleConcentrationBroken(MechanicEntity reason)
	{
		TryTrigger(reason);
	}
}
