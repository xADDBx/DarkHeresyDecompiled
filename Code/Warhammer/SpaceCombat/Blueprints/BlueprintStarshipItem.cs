using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("bc21b4860a4e0284eb894e8448f8958b")]
public abstract class BlueprintStarshipItem : BlueprintItemEquipment
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintStarshipItem>
	{
	}

	public StarshipEquipmentEntity StarshipEE;

	public bool IsBroken;

	[SerializeField]
	public List<BlueprintPartsCargo.Reference> AssembleItemRequirements;

	public int AssembleItemRequiredScrap;

	public int DisassembleScrapGiven;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}
}
