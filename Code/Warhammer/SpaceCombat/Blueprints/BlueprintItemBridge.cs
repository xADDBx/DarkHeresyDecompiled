using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("a9c2da5cdf1e96545b8b1932077442c7")]
public class BlueprintItemBridge : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemBridge>
	{
	}

	public override ItemsItemType ItemType => ItemsItemType.StarshipBridge;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
