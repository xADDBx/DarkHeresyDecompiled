using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Framework;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAreaEffect))]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[ComponentName("Scaling/PropertyScalingComponent")]
[TypeId("a26cb4f4249b4154a0c6701ef9986a68")]
public sealed class PropertyScalingComponent : BlueprintComponent
{
	public PropertyCalculator Calculator;

	public LocalizedString Description;
}
