using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.Items.Utility;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[ComponentName("Items/BlueprintItemContainerArmor")]
[TypeId("2ad7218e7ff5d5744a55bad286adc303")]
public sealed class BlueprintItemContainerArmor : BlueprintItemArmor, IBlueprintItemContainer
{
	[ValidateNotNull]
	[SerializeField]
	private BpRef<BlueprintItemArmor> _item = new BpRef<BlueprintItemArmor>();

	[SerializeField]
	private CRToPowerLevelEntry[] _crToPowerLevelOverride = Array.Empty<CRToPowerLevelEntry>();

	[SerializeField]
	private PowerLevelToArmorEntry[] _powerLevelToItemOverride = Array.Empty<PowerLevelToArmorEntry>();

	[SerializeField]
	private ItemFaction _overrideFaction;

	private PowerLevelToItemEntry[] _powerLevelToItemOverrideView;

	BlueprintItem IBlueprintItemContainer.DefaultConcreteItem => (BlueprintItemArmor?)_item;

	CRToPowerLevelEntry[] IBlueprintItemContainer.CRToPowerLevelOverride => _crToPowerLevelOverride;

	IReadOnlyList<PowerLevelToItemEntry> IBlueprintItemContainer.PowerLevelToItemOverride => _powerLevelToItemOverrideView ?? (_powerLevelToItemOverrideView = BuildOverrideView(_powerLevelToItemOverride));

	ItemFaction IBlueprintItemContainer.OverrideFaction => _overrideFaction;

	private static PowerLevelToItemEntry[] BuildOverrideView(PowerLevelToArmorEntry[] source)
	{
		PowerLevelToItemEntry[] array = new PowerLevelToItemEntry[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			array[i] = new PowerLevelToItemEntry(source[i].PowerLevel, (BlueprintItemArmor?)source[i].Item);
		}
		return array;
	}
}
