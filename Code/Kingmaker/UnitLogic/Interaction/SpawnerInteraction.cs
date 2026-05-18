using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Spawners;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[RequireComponent(typeof(UnitSpawnerBase))]
[KnowledgeDatabaseID("95d03dcfa566f5a4cadd0988cd234972")]
public abstract class SpawnerInteraction : EntityPartComponent<SpawnerInteractionPart>
{
	[Header("General")]
	public int OverrideDistance;

	[ShowCreator]
	public ConditionsReference Conditions;

	public bool AllowInCombat;

	public bool TriggerOnApproach;

	[ShowIf("TriggerOnApproach")]
	public bool TriggerOnParty = true;

	[ShowIf("TriggerOnApproach")]
	public float Cooldown = 5f;

	public LocalizedString DisplayName;

	[ShowIf("TriggerOnApproach")]
	public GlobalCooldownSettings GlobalCooldown;

	public abstract bool IsDialog { get; }

	public abstract AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
