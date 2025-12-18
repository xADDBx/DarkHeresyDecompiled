using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[TypeId("47c5290bbb5445c4be73aa66f24f382f")]
public class BlueprintWeaponStyleAnimationSet : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintWeaponStyleAnimationSet>
	{
	}

	[Header("Locomotion")]
	public WeaponStyleLocomotionData Locomotion;

	[Header("Cover")]
	public WeaponStyleCoverData Cover;

	[Header("Actions")]
	public WeaponStyleLeapData Leap;

	public WeaponStyleClimbData Climb;

	public WeaponStyleAbilityData Ability;

	public WeaponStyleAttackData Attack;

	public WeaponStyleEquipData Equip;

	public WeaponStyleCustomLoopActionData CustomLoopAction;

	[Header("Reactions")]
	public WeaponStyleDefenceData Defence;

	[Header("Disabled")]
	public WeaponStyleDisabledData Disabled;

	public WeaponStyleProneData Prone;

	[Header("Doll Room")]
	public WeaponStyleDollRoomData DollRoom;
}
