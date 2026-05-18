using System;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Serializable]
[TypeId("055f5a025ed7a1c43ad3a4cb666ae939")]
public class IsInteractionsOnGlobalCooldown : Condition
{
	protected override string GetConditionCaption()
	{
		return "Interactions are on global cooldown";
	}

	protected override bool CheckCondition()
	{
		InteractionGlobalCooldownController controller = Game.Instance.GetController<InteractionGlobalCooldownController>();
		if (controller == null)
		{
			return false;
		}
		return !controller.CheckGlobalCooldown();
	}
}
