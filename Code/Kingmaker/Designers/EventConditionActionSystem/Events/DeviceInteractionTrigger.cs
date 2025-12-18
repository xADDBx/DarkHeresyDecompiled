using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[ComponentName("Events/DeviceInteractionTrigger")]
[AllowMultipleComponents]
[TypeId("43b80d49327670546b2bf126ecef849b")]
public class DeviceInteractionTrigger : EntityFactComponentDelegate, IInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public ActionList Actions;

	public ActionList RestrictedActions;

	public void OnInteract(AbstractInteractionPart interaction)
	{
		MapObjectView mapObjectView = interaction.View.Or(null);
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void OnInteractionRestricted(AbstractInteractionPart interaction)
	{
		if (!RestrictedActions.HasActions)
		{
			return;
		}
		MapObjectView mapObjectView = interaction.View.Or(null);
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				RestrictedActions.Run();
			}
		}
	}
}
