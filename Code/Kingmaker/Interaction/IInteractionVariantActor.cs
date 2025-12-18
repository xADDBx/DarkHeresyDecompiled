using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.Interaction;

public interface IInteractionVariantActor : IInteractionRestriction
{
	int? InteractionDC { get; }

	InteractionActorType Type { get; }

	UIInteractionType UIType { get; }

	AbstractInteractionPart InteractionPart { get; }

	BlueprintAdditionalCombatObjective CombatObjective { get; }

	bool ShowInteractFx { get; }

	int? RequiredItemsCount { get; }

	BlueprintItem RequiredItem { get; }

	StatType Skill { get; }

	bool CheckOnlyOnce { get; }

	bool CanUse { get; }

	bool AlreadyUsed { get; }

	[CanBeNull]
	string GetInteractionName();

	void OnDidInteract(BaseUnitEntity user);

	void OnFailedInteract(BaseUnitEntity user);

	bool TryInteract(BaseUnitEntity user);
}
