using System;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("30bbe0cad8bef9b4491cf77ad3e93cc8")]
public class TutorialTriggerUnitCondition : TutorialTrigger
{
	public UnitCondition TriggerCondition;

	public void HandleUnitConditionsChanged(UnitCondition condition)
	{
	}
}
