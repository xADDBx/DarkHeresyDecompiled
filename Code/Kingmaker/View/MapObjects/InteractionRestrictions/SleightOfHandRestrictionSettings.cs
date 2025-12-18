using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class SleightOfHandRestrictionSettings : SkillUseRestrictionSettings
{
	public override StatType GetSkill()
	{
		return StatType.SkillSleightOfHand;
	}

	public override BlueprintItem GetItem()
	{
		return null;
	}

	public SleightOfHandRestrictionSettings()
	{
		Difficulty = SkillCheckDifficulty.Normal;
	}
}
