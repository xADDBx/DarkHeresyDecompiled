using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[Obsolete]
[TypeId("438d80c41f21b36409f5fc224d8ed63e")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentBelt : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Belt;
}
