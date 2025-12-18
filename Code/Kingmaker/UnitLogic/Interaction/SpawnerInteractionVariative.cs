using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[KnowledgeDatabaseID("e3b1f2a36d4e4c86a5f1dbe02f33c618")]
public class SpawnerInteractionVariative : SpawnerInteraction
{
	[SerializeField]
	[InspectorReadOnly]
	[Obsolete]
	private List<InteractionSkillCheck> m_Variants = new List<InteractionSkillCheck>();

	[SerializeField]
	public List<InteractionWithConditions> InteractionsWithConditions;

	public override bool IsDialog => false;

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		IEnumerable<InteractionActorWithConditions> interactionsWithConditions = InteractionsWithConditions.Select((InteractionWithConditions iwc) => iwc.ToActorWithConditions());
		EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
		{
			h.HandleInteractionRequest(target, interactionsWithConditions);
		});
		return AbstractUnitCommand.ResultType.None;
	}
}
