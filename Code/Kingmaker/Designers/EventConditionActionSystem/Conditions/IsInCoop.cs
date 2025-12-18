using Kingmaker.ElementsSystem;
using Kingmaker.Networking;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("b62f7e34c1de4544b5280e62e30dff58")]
public class IsInCoop : Condition
{
	protected override string GetConditionCaption()
	{
		return "Player is in coop mode";
	}

	protected override bool CheckCondition()
	{
		return NetworkingManager.IsActive;
	}
}
