using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.Items.Utility;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Weapons;

[ComponentName("Items/BlueprintItemContainerWeapon")]
[TypeId("a4f51eb24d105d8418b5de88c70b75d0")]
public sealed class BlueprintItemContainerWeapon : BlueprintItemWeapon, IBlueprintItemContainer
{
	[ValidateNotNull]
	[SerializeField]
	private BpRef<BlueprintItemWeapon> _item = new BpRef<BlueprintItemWeapon>();

	[SerializeField]
	private CRToPowerLevelEntry[] _crToPowerLevelOverride = Array.Empty<CRToPowerLevelEntry>();

	[SerializeField]
	private PowerLevelToWeaponEntry[] _powerLevelToItemOverride = Array.Empty<PowerLevelToWeaponEntry>();

	[SerializeField]
	private ItemFaction _overrideFaction;

	private PowerLevelToItemEntry[] _powerLevelToItemOverrideView;

	BlueprintItem IBlueprintItemContainer.DefaultConcreteItem => (BlueprintItemWeapon?)_item;

	CRToPowerLevelEntry[] IBlueprintItemContainer.CRToPowerLevelOverride => _crToPowerLevelOverride;

	IReadOnlyList<PowerLevelToItemEntry> IBlueprintItemContainer.PowerLevelToItemOverride => _powerLevelToItemOverrideView ?? (_powerLevelToItemOverrideView = BuildOverrideView(_powerLevelToItemOverride));

	ItemFaction IBlueprintItemContainer.OverrideFaction => _overrideFaction;

	private static PowerLevelToItemEntry[] BuildOverrideView(PowerLevelToWeaponEntry[] source)
	{
		PowerLevelToItemEntry[] array = new PowerLevelToItemEntry[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			array[i] = new PowerLevelToItemEntry(source[i].PowerLevel, (BlueprintItemWeapon?)source[i].Item);
		}
		return array;
	}
}
