using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentRing")]
[TypeId("54663362c56fb114c9dc709407dcfc82")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentRing : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Ring;
}
