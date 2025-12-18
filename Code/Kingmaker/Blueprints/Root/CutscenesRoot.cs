using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[ComponentName("Root/CutscenesRoot")]
[TypeId("eeaa8d85bf3588d4f9123384bd993309")]
public class CutscenesRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<CutscenesRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_AttackSingle;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_AttackBurst;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_AttackSpell;

	[Obsolete("WH2-7940")]
	[HideInInspector]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityFXSettings.Reference m_SpellWeaponFXSettings;

	[SerializeField]
	private BlueprintItemWeaponReference m_DefaultMeleeWeapon;

	[SerializeField]
	private BlueprintItemWeaponReference m_DefaultRangeWeapon;

	[SerializeField]
	private BpRef<BlueprintCutscene> m_DefaultElevatorCutscene;

	public BlueprintAbility AttackSingle => m_AttackSingle;

	public BlueprintAbility AttackBurst => m_AttackBurst;

	public BlueprintAbility AttackSpell => m_AttackSpell;

	[Obsolete("WH2-7940")]
	public BlueprintAbilityFXSettings.Reference SpellWeaponFXSettings => m_SpellWeaponFXSettings;

	public BlueprintItemWeapon DefaultWeaponMelee => m_DefaultMeleeWeapon;

	public BlueprintItemWeapon DefaultWeaponRanged => m_DefaultRangeWeapon;

	public BlueprintCutscene DefaultElevatorCutscene => m_DefaultElevatorCutscene;
}
