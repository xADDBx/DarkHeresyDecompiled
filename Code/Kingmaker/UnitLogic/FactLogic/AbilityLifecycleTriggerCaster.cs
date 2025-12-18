using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[ComponentName("Ability/AbilityLifecycleTriggerCaster")]
[TypeId("6ebdb034bf2f486b932f9342e175ec6d")]
public class AbilityLifecycleTriggerCaster : AbilityLifecycleTrigger, IAbilityExecutionProcessHandler<EntitySubscriber>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessHandler, EntitySubscriber>
{
	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		RunStartActions(context);
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		RunEndActions(context);
	}
}
