using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Interaction;

[DisallowMultipleComponent]
[KnowledgeDatabaseID("e3b1f2a36d4e4c86a5f1dbe02f33c618")]
public class SpawnerInteractionVariative : SpawnerInteraction
{
	[FormerlySerializedAs("InteractionsWithConditions")]
	[Header("Interactions")]
	[SerializeField]
	public List<InteractionWithConditions> Interactions;

	public override bool IsDialog => false;

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		List<InteractionActorWithConditions> interactionsWithConditions;
		using (CollectionPool<List<InteractionActorWithConditions>, InteractionActorWithConditions>.Get(out interactionsWithConditions))
		{
			foreach (InteractionActorWithConditions item in Interactions.Select((InteractionWithConditions iwc) => iwc.ToActorWithConditions()))
			{
				interactionsWithConditions.Add(item);
			}
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(target, interactionsWithConditions);
			});
			return AbstractUnitCommand.ResultType.Success;
		}
	}
}
