namespace Kingmaker.RuleSystem.Rules;

public static class SkillCheckTypeExtensions
{
	public static bool HasAnyFlag(this SkillCheckTypeFlags flags, SkillCheckTypeFlags type)
	{
		return (flags & type) != 0;
	}

	public static bool HasAnyFlag(this SkillCheckTypeFlags flags, SkillCheckType type)
	{
		return flags.HasAnyFlag((SkillCheckTypeFlags)(1 << (int)type));
	}
}
