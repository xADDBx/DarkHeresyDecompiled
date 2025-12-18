using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[TypeId("c4fc6e2512c31b246b530ab2d90eb2fa")]
public class ExperienceTrigger : EntityFactComponentDelegate
{
	public int Experience;

	public ConditionsChecker Conditions;

	public ActionList Actions;
}
