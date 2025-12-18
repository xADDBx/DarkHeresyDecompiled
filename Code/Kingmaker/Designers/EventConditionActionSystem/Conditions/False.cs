using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/False")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("f3e94df96a3153f4eb5a5c97dfa322e8")]
public class False : Condition
{
	protected override string GetConditionCaption()
	{
		return "False";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
