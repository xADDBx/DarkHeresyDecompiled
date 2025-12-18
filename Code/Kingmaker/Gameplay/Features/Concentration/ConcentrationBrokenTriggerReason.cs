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
[TypeId("f70a8e1aacc74a51baf0ccedaa8189bf")]
[ClassInfoBox("Срабатывает когда Owner сбил кому-то концентрацию")]
[ComponentName("Concentration/ConcentrationBrokenTriggerReason")]
public class ConcentrationBrokenTriggerReason : ConcentrationBrokenTrigger, IConcentrationBrokenHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	void IConcentrationBrokenHandler.HandleConcentrationBroken(MechanicEntity reason)
	{
		if (reason == base.Owner)
		{
			TryTrigger(reason);
		}
	}
}
