using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[TypeId("aa152791993a4dd4d8d24075385b9e3e")]
public class TimeOfDayChangedTrigger : EntityFactComponentDelegate, ITimeOfDayChangedHandler, ISubscriber
{
	public ActionList Actions;

	public void OnTimeOfDayChanged()
	{
		Actions.Run();
	}
}
