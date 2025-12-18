using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("5896161b9357eb64f9bba48be98e1bf7")]
public class BlueprintItemLifeSustainer : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemLifeSustainer>
	{
	}

	public override ItemsItemType ItemType => ItemsItemType.StarshipLifeSustainer;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
