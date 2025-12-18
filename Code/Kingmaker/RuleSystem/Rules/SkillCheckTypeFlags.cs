using System;

namespace Kingmaker.RuleSystem.Rules;

[Flags]
public enum SkillCheckTypeFlags
{
	Default = 1,
	CritSave = 2,
	Inspect = 4,
	DOT = 8
}
