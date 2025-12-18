using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("fdac0d09c25704d448daaccbfc121d94")]
public class BlueprintItemGellerFieldDevice : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemGellerFieldDevice>
	{
	}

	public override ItemsItemType ItemType => ItemsItemType.StarshipGellerFieldDevice;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
