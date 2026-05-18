using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("af629def8de3a114dae6fa88f508352b")]
public class TutorialTriggerSignalDeviceInterface : TutorialTrigger, ISignalDeviceShownHandler, ISubscriber
{
	private bool m_IsTriggered;

	public void HandleSignalDeviceShown()
	{
		if (!m_IsTriggered)
		{
			TryToTrigger(null);
			m_IsTriggered = true;
		}
	}
}
