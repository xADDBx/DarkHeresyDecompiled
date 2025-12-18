using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class RitualSetRestrictionSettings : NeedItemRestrictionSettings
{
	public override BlueprintItem GetItem()
	{
		return ConfigRoot.Instance.Consumables.RitualSetItem;
	}
}
