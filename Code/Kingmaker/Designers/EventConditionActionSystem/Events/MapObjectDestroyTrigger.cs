using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[TypeId("ac865782c9f5f894784b9f7f0b722def")]
public class MapObjectDestroyTrigger : EntityFactComponentDelegate
{
	public ActionList DestroyedActions;

	public ActionList DestructionFailedActions;
}
