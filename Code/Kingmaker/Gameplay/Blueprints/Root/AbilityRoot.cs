using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Blueprints.Root;

[Serializable]
[ComponentName("Root/AbilityRoot")]
[TypeId("c079ddc3844245878b2f601571d8616d")]
public sealed class AbilityRoot : BlueprintScriptableObject, IScanOnBuild, IBlueprintScanner
{
	[ValidateNotNull]
	public BpRef<BlueprintAbilityTag> WeaponSingleShotTag = new BpRef<BlueprintAbilityTag>();

	[ValidateNotNull]
	public BpRef<BlueprintAbilityTag> WeaponBurstTag = new BpRef<BlueprintAbilityTag>();

	[ValidateNotNull]
	public BpRef<BlueprintAbilityTag> WeaponAoETag = new BpRef<BlueprintAbilityTag>();

	[ValidateNotNull]
	[Tooltip("This tag is needed to indicate weapon tags on the modifiers screen")]
	public BpRef<BlueprintAbilityTag> AttackAbilityTag = new BpRef<BlueprintAbilityTag>();

	[ValidateNotNull]
	[Tooltip("ToggleAbility applies modifier effects on turn start if has this tag")]
	public BpRef<BlueprintAbilityTag> BuffAbilityTag = new BpRef<BlueprintAbilityTag>();

	[ValidateNotNull]
	public BpRef<BlueprintAbilityTag> UniversalTag = new BpRef<BlueprintAbilityTag>();

	[ValidateNotNull]
	[Tooltip("ToggleAbility with tag Buff must apply bound modifiers at the start of the turn")]
	public BpRef<BlueprintAbility> ToggleAbilityStartTurnModifiers = new BpRef<BlueprintAbility>();

	[InspectorReadOnly]
	public BpRef<BlueprintAbilityTag>[] AllTags = new BpRef<BlueprintAbilityTag>[0];

	void IScanOnBuild.Scan()
	{
		Scan();
	}

	void IBlueprintScanner.Scan()
	{
		Scan();
	}

	private void Scan()
	{
	}
}
