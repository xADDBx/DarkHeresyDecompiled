using System;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.View.MapObjects;

public class SkillCheckActualDifficultyAttribute : EnumOrderAttribute
{
	private static readonly Enum[] s_order = new Enum[5]
	{
		SkillCheckDifficulty.Easy,
		SkillCheckDifficulty.Normal,
		SkillCheckDifficulty.Hard,
		SkillCheckDifficulty.Impossible,
		SkillCheckDifficulty.Custom
	};

	public override Enum[] Order => s_order;
}
