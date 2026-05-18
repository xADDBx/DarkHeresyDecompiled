using System;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Interaction;

[Serializable]
[TypeId("a5ccdeff1718ec946a6b7f7de499dd64")]
public sealed class ClickUnitActions : UnitInteractionComponent
{
	public ActionList Actions = new ActionList();

	public override bool IsDialog => Actions.Actions.HasItem((GameAction i) => i is StartDialog);

	public override bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		if (!base.IsAvailable(initiator, target))
		{
			return false;
		}
		return true;
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		if (user.IsInCombat)
		{
			if (user.CombatState.ActionPoints < InCombatActionCost)
			{
				return AbstractUnitCommand.ResultType.Fail;
			}
			user.CombatState.SpendActionPoints(InCombatActionCost);
		}
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				Actions.Run();
			}
		}
		EventBus.RaiseEvent((IMechanicEntity)target, (Action<IUnitClickHandler>)delegate(IUnitClickHandler h)
		{
			h.HandleClickOnUnit();
		}, isCheckRuntime: true);
		return AbstractUnitCommand.ResultType.Success;
	}
}
