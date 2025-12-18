using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("c12d0b47a00427c47be7af83bb98bf3c")]
public class OverrideAbilityWeapon : MechanicEntityFactComponentDelegate
{
	[SerializeField]
	private BlueprintItemWeaponReference m_Weapon;

	public BlueprintItemWeapon Weapon => m_Weapon?.Get();
}
