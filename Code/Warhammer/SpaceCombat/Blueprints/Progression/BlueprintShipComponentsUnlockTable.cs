using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Warhammer.SpaceCombat.Blueprints.Progression;

[Obsolete]
[TypeId("53dab683877e45f19422861f1b0d603b")]
public class BlueprintShipComponentsUnlockTable : BlueprintScriptableObject
{
	[Serializable]
	public struct UnlockTable
	{
		[SerializeField]
		public int Level;

		[SerializeField]
		public BlueprintStarshipItem.Reference ShipComponent;

		[SerializeField]
		[Tooltip("Fill this if Item is weapon")]
		public WeaponSlotType WeaponSlotType;

		[SerializeField]
		[Tooltip("If insert prow, then set on which side to equip. Uncheck - Left. Check - Right")]
		public bool ProwInsertRightSlot;
	}

	[SerializeField]
	public UnlockTable[] Table;
}
