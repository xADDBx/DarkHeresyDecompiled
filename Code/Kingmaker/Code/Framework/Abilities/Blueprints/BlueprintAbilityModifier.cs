using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Utility;
using Kingmaker.Framework.Utility;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Code.Framework.Abilities.Blueprints;

[Serializable]
[ComponentName("Ability/BlueprintAbilityModifier")]
[TypeId("85050fc3e2204a5d9348dde0594cc3b0")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintAbilityModifier : BlueprintMechanicEntityFact
{
	[SerializeField]
	private AbilityModifierTags _tags = new AbilityModifierTags();

	[InfoBox("Этот блок настроек - переопределение параметров абилки, к которой залинкован Modifier")]
	public OptionalValue<AbilityRange> Range = new OptionalValue<AbilityRange>();

	public OptionalValue<int> CustomRange = new OptionalValue<int>();

	public OptionalValue<int> MinRange = new OptionalValue<int>();

	public OptionalValue<int> ActionPointCost = new OptionalValue<int>();

	public OptionalValue<int> VeilDamage = new OptionalValue<int>();

	public OptionalValue<int> CooldownRounds = new OptionalValue<int>();

	public OptionalValue<bool> CanTargetSelf = new OptionalValue<bool>();

	public OptionalValue<bool> CanTargetDestructibleObjects = new OptionalValue<bool>();

	public OptionalValue<bool> CanTargetEnemies = new OptionalValue<bool>();

	public OptionalValue<bool> CanTargetFriends = new OptionalValue<bool>();

	public OptionalValue<bool> CanTargetPoint = new OptionalValue<bool>();

	public OptionalValue<BlueprintAbility.UsingInThreateningAreaType> UsingInThreateningArea = new OptionalValue<BlueprintAbility.UsingInThreateningAreaType>();

	public override bool AllowContextActionsOnly => true;

	public IEnumerable<BlueprintAbilityTag> Tags => _tags;

	public AbilityModifierTags ModifierTags => _tags;

	public bool Match(BpRefArray<BlueprintAbilityTag> tags)
	{
		return _tags.Match(tags);
	}

	public bool Match(BlueprintAbilityTag tag)
	{
		return _tags.Match(tag);
	}
}
