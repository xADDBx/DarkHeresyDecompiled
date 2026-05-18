using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Spawners;

namespace Kingmaker.UnitLogic.Interaction;

[KnowledgeDatabaseID("4dc8ec633041a694e9994df48d4e7a09")]
public class SpawnerInteractionActions : SpawnerInteraction
{
	public List<ActionsReference> ActionHolders = new List<ActionsReference>();

	public override bool IsDialog => ActionHolders.NotNull().Any(delegate(ActionsReference i)
	{
		ActionsHolder actionsHolder = i.Get();
		if (actionsHolder != null)
		{
			ActionList actions = actionsHolder.Actions;
			if (actions != null)
			{
				GameAction[] actions2 = actions.Actions;
				if (actions2 != null)
				{
					return actions2.HasItem((GameAction a) => a is StartDialog);
				}
			}
		}
		return false;
	});

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				using (ContextData<SpawnedUnitData>.Request().Setup(GetComponent<UnitSpawnerBase>().SpawnedUnit, GetComponent<UnitSpawnerBase>().SpawnedUnit.HoldingState))
				{
					using (ContextData<ActionExecutionContextData>.Request().Setup(ActionExecutionContextData.Type.Interaction))
					{
						foreach (ActionsReference actionHolder in ActionHolders)
						{
							if (actionHolder?.Get() != null)
							{
								actionHolder.Get().Actions.Run();
							}
						}
					}
				}
			}
		}
		return AbstractUnitCommand.ResultType.Success;
	}
}
