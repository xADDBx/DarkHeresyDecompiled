using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items;

[ComponentName("Items/BlueprintItemResourceMiner")]
[TypeId("cba5145ec4e04f15ab3f3215b26eee0f")]
public class BlueprintItemResourceMiner : BlueprintItem
{
	public override ItemsItemType ItemType => ItemsItemType.ResourceMiner;
}
