using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[AllowMultipleComponents]
[TypeId("95e41903dacc47f3a21c98338688266f")]
public class AreaTransitionTrigger : EntityFactComponentDelegate, IAreaTransitionHandler, ISubscriber
{
	public ActionList Actions;

	public ConditionsChecker Conditions;

	public void HandleAreaTransition()
	{
		using (base.Fact.MaybeContext?.SetScope())
		{
			if (Conditions.Check())
			{
				Actions.Run();
			}
		}
	}
}
