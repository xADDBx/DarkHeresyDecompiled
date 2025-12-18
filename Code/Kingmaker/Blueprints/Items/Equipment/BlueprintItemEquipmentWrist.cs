using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Blueprints.Items.Equipment;

[Obsolete]
[TypeId("a7df4d922aaf5054c80bc92281ebfd57")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentWrist : BlueprintItemEquipmentSimple
{
	public override ItemsItemType ItemType => ItemsItemType.Wrist;
}
