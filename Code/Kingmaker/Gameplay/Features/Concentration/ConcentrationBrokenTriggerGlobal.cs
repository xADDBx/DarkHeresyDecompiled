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
[TypeId("7da3e0912836495988836f976932b79d")]
[ClassInfoBox("Срабатывает когда кто угодно сбил кому угодно концентрацию")]
[ComponentName("Concentration/ConcentrationBrokenTriggerGlobal")]
public class ConcentrationBrokenTriggerGlobal : ConcentrationBrokenTrigger, IConcentrationBrokenHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	void IConcentrationBrokenHandler.HandleConcentrationBroken(MechanicEntity reason)
	{
		TryTrigger(reason);
	}
}
