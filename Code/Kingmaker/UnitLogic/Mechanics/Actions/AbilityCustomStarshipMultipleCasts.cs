using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("dd15ee9b51e103e428b6ac965fd1a737")]
public class AbilityCustomStarshipMultipleCasts : BlueprintComponent
{
	[SerializeField]
	private BlueprintAbilityReference m_AttackAbility;

	[SerializeField]
	private int repeatsMin;

	[SerializeField]
	private int repeatsMax;

	[SerializeField]
	private float pauseAfterEachCast;

	[SerializeField]
	private float finalPause;

	public BlueprintAbility AttackAbility => m_AttackAbility.Get();
}
