using System;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[TypeId("f03a0e54184bb4b48afa2fcfe640d5e7")]
public class MapObjectAwarenessTrigger : EntityFactComponentDelegate, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber
{
	public ActionList Actions;

	public void OnEntityNoticed(BaseUnitEntity character)
	{
		MapObjectView mapObjectView = EventInvokerExtensions.GetEntity<MapObjectEntity>().View as MapObjectView;
		if (mapObjectView == null || !mapObjectView.IsOwnerOf(base.Runtime))
		{
			return;
		}
		using (ContextData<SpotterData>.Request().Setup(character))
		{
			using (ContextData<MechanicEntityData>.Request().Setup(mapObjectView.Data))
			{
				Actions.Run();
			}
		}
	}
}
