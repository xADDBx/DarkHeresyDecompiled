using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("ce6032129297463facc9f76a42fc0aa1")]
public class TutorialTriggerIUnitTraumaReceived : TutorialTrigger, IUnitTraumaHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public void HandleTraumaReceived()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.IsInPlayerParty)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = EventInvokerExtensions.BaseUnitEntity;
			});
		}
	}
}
