using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[Obsolete]
[TypeId("48da549c948b0bc4882440cf71ec2b50")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentShirt : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Shirt;
}
