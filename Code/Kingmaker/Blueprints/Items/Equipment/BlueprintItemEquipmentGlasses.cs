using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[Obsolete]
[TypeId("f5333b2b3a1310b45846a6d044b6550d")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentGlasses : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Glasses;
}
