using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentNeck")]
[TypeId("c5833a3cc6524c84ea8a97b09dff8b8f")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentNeck : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Neck;
}
