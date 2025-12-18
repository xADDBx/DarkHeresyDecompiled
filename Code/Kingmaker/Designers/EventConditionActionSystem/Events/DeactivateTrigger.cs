using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/DeactivateTrigger")]
[AllowMultipleComponents]
[TypeId("b2970e0dadd194546bbf27a691e55d4c")]
public class DeactivateTrigger : EntityFactComponentDelegate
{
	public ConditionsChecker Conditions;

	public ActionList Actions;

	protected override void OnDeactivate()
	{
		if (Conditions.Check())
		{
			Actions.Run();
		}
	}
}
