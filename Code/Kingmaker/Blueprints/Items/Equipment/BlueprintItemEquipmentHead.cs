using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentHead")]
[TypeId("adc63a0dcf487a44c9c7e8ccb4b5fb33")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentHead : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Head;
}
