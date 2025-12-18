using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("982407ed6d4c4b34c9e135d2d1c79859")]
public class StarshipActionsOnUnits : BlueprintComponent
{
	public bool ignoreSoft;

	public int repeatsMin;

	public int repeatsMax;

	[SerializeField]
	private BlueprintProjectileReference m_Projectile;

	public ActionList Actions;

	public BlueprintProjectile Projectile => m_Projectile?.Get();
}
