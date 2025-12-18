using System;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("13126ef8cda1b74478b3536cc6bbe28f")]
public class EtudeBracketOverrideActionsOnClick : EtudeBracketOverrideInteraction
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public ActionList Actions = new ActionList();

	public override bool IsDialog
	{
		get
		{
			ActionList actions = Actions;
			if (actions != null)
			{
				GameAction[] actions2 = actions.Actions;
				if (actions2 != null)
				{
					return actions2.HasItem((GameAction i) => i is StartDialog);
				}
			}
			return false;
		}
	}

	protected override void OnEnter()
	{
		PartUnitInteractions orCreate = Unit.GetValue().GetOrCreate<PartUnitInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Interaction = new EtudeBracketOverrideUnitInteraction(this);
		orCreate.AddInteraction(componentData.Interaction);
	}

	protected override void OnExit()
	{
		PartUnitInteractions orCreate = Unit.GetValue().GetOrCreate<PartUnitInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		orCreate.RemoveInteraction(componentData.Interaction);
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				Actions.Run();
			}
		}
		return AbstractUnitCommand.ResultType.Success;
	}
}
