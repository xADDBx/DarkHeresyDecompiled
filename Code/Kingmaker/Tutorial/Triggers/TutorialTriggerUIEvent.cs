using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[AllowMultipleComponents]
[TypeId("fa8dc9d6dc1bd454ea9210190aa0bd6b")]
public class TutorialTriggerUIEvent : TutorialTrigger, IUIEventHandler, ISubscriber
{
	public UIEventType UIEvent;

	public void HandleUIEvent(UIEventType type)
	{
		if (type == UIEvent)
		{
			TryToTrigger(null);
		}
	}
}
