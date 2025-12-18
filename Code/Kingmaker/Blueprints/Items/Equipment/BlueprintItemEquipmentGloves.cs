using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentGloves")]
[TypeId("ef955c4834e9e224b98936726b7348a8")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentGloves : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Gloves;
}
