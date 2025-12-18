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
		InteractionAction value = InteractionActionEvaluator.GetValue();
		if (TryCreateInteractCommand(baseUnitEntity, value, out var commandParams))
		{
			baseUnitEntity.Commands.RunImmediate(commandParams);
		}
	}

	private bool TryCreateInteractCommand(BaseUnitEntity agent, InteractionAction interaction, out UnitCommandParams commandParams)
	{
		commandParams = null;
		InteractionActionPart interactionActionPart = interaction.EnsurePart();
		if (interactionActionPart.Type == InteractionType.Flashlight || interactionActionPart.Type == InteractionType.Variant)
		{
			return false;
		}
		if (!interactionActionPart.CanInteract())
		{
			return false;
		}
		if (interactionActionPart.Type == InteractionType.Direct)
		{
			commandParams = UnitDirectInteract.CreateCommandParams(interactionActionPart);
			return true;
		}
		if (interactionActionPart.Type == InteractionType.Approach && interactionActionPart.IsEnoughCloseForInteraction(agent))
		{
			commandParams = new UnitInteractWithObjectParams(interactionActionPart)
			{
				IsSynchronized = true
			};
			return true;
		}
		return false;
	}
}
