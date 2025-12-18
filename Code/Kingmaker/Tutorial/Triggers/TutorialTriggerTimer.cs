using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("ff647c629c1143fab475eccdd0fee663")]
public abstract class TutorialTriggerTimer : EntityFactComponentDelegate, ITutorialTriggerTimerHandler, ISubscriber
{
	[SerializeField]
	protected int TimerValue;

	[SerializeField]
	protected ActionList Actions = new ActionList();

	protected bool CanStart;

	protected bool IsDone;

	public virtual void HandleTimerStart()
	{
		CanStart = true;
	}
}
