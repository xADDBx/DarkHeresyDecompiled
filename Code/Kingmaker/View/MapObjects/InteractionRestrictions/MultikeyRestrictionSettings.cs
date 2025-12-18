using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class MultikeyRestrictionSettings : NeedItemRestrictionSettings
{
	public override BlueprintItem GetItem()
	{
		return ConfigRoot.Instance.Consumables.MultikeyItem;
	}
}
