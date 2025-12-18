using System;

namespace Kingmaker.UnitLogic.Enums;

[Obsolete]
public enum UnitCondition
{
	Blindness = 1,
	Entangled = 11,
	Prone = 14,
	Sleeping = 16,
	Stunned = 23,
	CantMove = 19,
	CantAct = 28,
	DisableAttacksOfOpportunity = 29
}
