using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("f6ca6d5da1433d44a93b03a4c9bded9e")]
public class BlueprintItemArmorPlating : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemArmorPlating>
	{
	}

	public int ArmourFore;

	public int ArmourPort;

	public int ArmourStarboard;

	public int ArmourAft;

	public override ItemsItemType ItemType => ItemsItemType.StarshipArmorPlating;
}
