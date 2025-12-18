using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class LoreXenosMultikeyItemRestrictionSettings : SkillUseRestrictionSettings
{
	public override StatType GetSkill()
	{
		return StatType.SkillLoreXenos;
	}

	public override BlueprintItem GetItem()
	{
		return ConfigRoot.Instance.Consumables.MultikeyItem;
	}

	public LoreXenosMultikeyItemRestrictionSettings()
	{
		Difficulty = SkillCheckDifficulty.Normal;
	}
}
