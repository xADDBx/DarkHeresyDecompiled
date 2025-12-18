using System;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Equipment;

[Serializable]
public class WeaponAbility
{
	public WeaponAbilityType Type;

	[SerializeField]
	[HideIf("HideFields")]
	public BlueprintAbilityReference m_Ability;

	[SerializeField]
	[HideIf("HideFields")]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	[SerializeField]
	[HideIf("HideFields")]
	private AttackAnimationType m_AttackAnimationType;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[ShowIf("IsBurst")]
	public RPM Rpm;

	[FormerlySerializedAs("OnHitOverrideType")]
	[HideIf("HideFields")]
	public ModifiersOverrideType ModifiersOverrideType;

	[SerializeField]
	[ValidateNoNullEntries]
	[HideIf("HideFields")]
	public BpRef<BlueprintAbilityModifier>[] Modifiers = new BpRef<BlueprintAbilityModifier>[0];

	[HideIf("HideFields")]
	public int AP;

	[CanBeNull]
	public BlueprintAbility Ability => IsNone ? null : m_Ability;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public AttackAnimationType AttackAnimationType => m_AttackAnimationType;

	public bool IsNone => Type == WeaponAbilityType.None;

	private bool HideFields => Type == WeaponAbilityType.None;

	public bool IsBurst => Type == WeaponAbilityType.Burst;
}
