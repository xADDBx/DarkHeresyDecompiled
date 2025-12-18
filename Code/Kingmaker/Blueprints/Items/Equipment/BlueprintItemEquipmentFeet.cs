using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentFeet")]
[TypeId("ed54f6f482fd846419c0d7be48c8db92")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentFeet : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Feet;
}
