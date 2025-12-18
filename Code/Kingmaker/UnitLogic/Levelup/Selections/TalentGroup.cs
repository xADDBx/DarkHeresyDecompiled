using System;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Flags]
public enum TalentGroup
{
	Offense = 1,
	Survivability = 2,
	Support = 4,
	Morale = 8,
	Psychic = 0x10
}
