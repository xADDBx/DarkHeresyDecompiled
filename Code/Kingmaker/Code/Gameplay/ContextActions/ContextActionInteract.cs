using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.ContextActions;

[Serializable]
[TypeId("41d7d7da710e41b3aabf6f1886ac0a5d")]
public class ContextActionInteract : ContextAction
{
	[SerializeReference]
	public InteractionActionEvaluator InteractionActionEvaluator;

	public override string GetCaption()
	{
		return "Interact with [" + InteractionActionEvaluator?.GetCaption() + "]";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = base.Caster as BaseUnitEntity;
		InteractionActionPart value = InteractionActionEvaluator.GetValue();
		if (TryCreateInteractCommand(baseUnitEntity, value, out var commandParams))
		{
			baseUnitEntity.Commands.RunImmediate(commandParams);
		}
	}

	private bool TryCreateInteractCommand(BaseUnitEntity agent, InteractionActionPart interactionPart, out UnitCommandParams commandParams)
	{
		commandParams = null;
		if (interactionPart.Type == InteractionType.Flashlight || interactionPart.Type == InteractionType.Variant)
		{
			return false;
		}
		if (!interactionPart.CanInteract())
		{
			return false;
		}
		if (interactionPart.Type == InteractionType.Direct)
		{
			commandParams = UnitDirectInteract.CreateCommandParams(interactionPart);
			return true;
		}
		if (interactionPart.Type == InteractionType.Approach && interactionPart.IsEnoughCloseForInteraction(agent))
		{
			commandParams = new UnitInteractWithObjectParams(interactionPart)
			{
				IsSynchronized = true
			};
			return true;
		}
		return false;
	}
}
