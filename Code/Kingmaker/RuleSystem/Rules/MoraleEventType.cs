using System;

namespace Kingmaker.RuleSystem.Rules;

[Flags]
public enum MoraleEventType
{
	CombatStart = 1,
	TurnStart = 2,
	AllyDeath = 4,
	EnemyDeath = 8,
	LeaderAllyDeath = 0x10,
	LeaderEnemyDeath = 0x20,
	RestoreToRegular = 0x40,
	BecomeHeroic = 0x80,
	BecomeBroken = 0x100,
	ForcedChangeMorale = 0x200,
	ForcedChangeMoralePhase = 0x400,
	CombatEnd = 0x800,
	TraumaStacked = 0x1000
}
