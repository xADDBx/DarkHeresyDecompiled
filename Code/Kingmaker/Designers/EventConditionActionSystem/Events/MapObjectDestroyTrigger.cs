using System;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[TypeId("ac865782c9f5f894784b9f7f0b722def")]
public class MapObjectDestroyTrigger : EntityFactComponentDelegate, IDestructionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public ActionList DestroyedActions;

	public ActionList DestructionFailedActions;

	public void HandleDestructionSuccess(MapObjectView mapObjectView)
	{
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				DestroyedActions.Run();
			}
		}
	}

	public void HandleDestructionFail(MapObjectView mapObjectView)
	{
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				DestructionFailedActions.Run();
			}
		}
	}
}
