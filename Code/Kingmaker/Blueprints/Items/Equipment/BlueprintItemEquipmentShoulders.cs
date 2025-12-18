using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentShoulders")]
[TypeId("d2b20f73f4e701d4e9e34b8b67465048")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentShoulders : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Shoulders;
}
