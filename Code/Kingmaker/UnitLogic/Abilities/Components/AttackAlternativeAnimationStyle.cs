using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Visual.Animation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("dbc2c558fd814daaa755adc5dc92a1f8")]
public class AttackAlternativeAnimationStyle : BlueprintComponent
{
	[SerializeField]
	private AnimationAlternativeStyle m_WeaponAnimationStyle;

	public AnimationAlternativeStyle WeaponAnimationStyle => m_WeaponAnimationStyle;
}
