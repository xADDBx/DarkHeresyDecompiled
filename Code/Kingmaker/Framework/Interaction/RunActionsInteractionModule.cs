using System;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Framework.Interaction;

[Serializable]
public sealed class RunActionsInteractionModule : InteractionModule
{
	[ShowCreator]
	public ActionsReference Actions = new ActionsReference();

	public override string GetCaption()
	{
		return "Run actions";
	}

	public override Task Execute(BaseUnitEntity initiator, MapObjectEntity target)
	{
		ActionsHolder actionsHolder = Actions.Get();
		if (actionsHolder != null && actionsHolder.HasActions)
		{
			using (ContextData<ActionExecutionContextData>.Request().Setup(ActionExecutionContextData.Type.Interaction))
			{
				using (ContextData<MechanicEntityData>.Request().Setup(target))
				{
					using (ContextData<InteractingUnitData>.Request().Setup(initiator))
					{
						actionsHolder.Run();
					}
				}
			}
		}
		return Task.CompletedTask;
	}
}
