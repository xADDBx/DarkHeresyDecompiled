using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("9a24abe68b113a649a9daec8b27a4357")]
public class BlueprintItemWarpDrives : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemWarpDrives>
	{
	}

	public override ItemsItemType ItemType => ItemsItemType.StarshipWarpDrives;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
