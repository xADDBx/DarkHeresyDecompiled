using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("10e0c4e16b29cf24ba962c87a49146d8")]
public class BlueprintItemAugerArray : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemAugerArray>
	{
	}

	[Header("Attack Bonuses")]
	[Range(0f, 100f)]
	public int hitChances;

	[Range(0f, 100f)]
	public int critChances;

	public override ItemsItemType ItemType => ItemsItemType.StarshipAugerArray;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
