using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class SleightOfHandMultikeyItemRestrictionSettings : SkillUseRestrictionSettings
{
	public override StatType GetSkill()
	{
		return StatType.SkillSleightOfHand;
	}

	public override BlueprintItem GetItem()
	{
		return ConfigRoot.Instance.Consumables.MultikeyItem;
	}
}
