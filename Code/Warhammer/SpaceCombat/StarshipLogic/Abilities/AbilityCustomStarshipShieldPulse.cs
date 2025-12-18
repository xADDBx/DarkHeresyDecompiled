using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9dc80916f1707e44dad5e7780e90efaf")]
public class AbilityCustomStarshipShieldPulse : BlueprintComponent
{
	[SerializeField]
	private float hitDelay;

	[SerializeField]
	private int ourShieldPctSpent;

	[SerializeField]
	private int aeldariHoloFieldPctRemoved;

	[SerializeField]
	private int aeldariMinimumRemoved;

	[SerializeField]
	private BlueprintBuffReference m_AeldariHoloFieldBuff;

	[SerializeField]
	private BlueprintFeatureReference m_DrukhariShadowFieldMark;

	[SerializeField]
	private BlueprintBuffReference m_NecronFallingApartBuff;

	[SerializeField]
	private int fallingApartShipHullDamagePct;

	[SerializeField]
	private ActionList ActionsOnSelf;

	public BlueprintBuff AeldariHoloFieldBuff => m_AeldariHoloFieldBuff?.Get();

	public BlueprintFeature DrukhariShadowFieldMark => m_DrukhariShadowFieldMark?.Get();

	public BlueprintBuff NecronFallingApartBuff => m_NecronFallingApartBuff?.Get();
}
