using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("`SourceItem` - item that no one in party can identify")]
[TypeId("60f0c0aee4458644d86d73292ad67f65")]
public class TutorialTriggerItemIdentificationFailed : TutorialTrigger, IIdentifyHandler, ISubscriber<IItemEntity>, ISubscriber
{
	public void OnItemIdentified(BaseUnitEntity character)
	{
	}

	public void OnFailedToIdentify()
	{
		TryToTrigger(null, delegate(TutorialContext x)
		{
			x.SourceItem = EventInvokerExtensions.GetEntity<ItemEntity>();
		});
	}
}
