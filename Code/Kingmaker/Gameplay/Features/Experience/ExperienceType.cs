using System;
using UnityEngine.Serialization;

namespace Kingmaker.Gameplay.Features.Experience;

public enum ExperienceType
{
	[FormerlySerializedAs("Mob")]
	Encounter = 0,
	SkillCheck = 1,
	Quest = 2,
	Investigation = 4,
	[Obsolete]
	Boss = 5,
	[Obsolete]
	QuestMain = 6,
	[Obsolete]
	QuestNormal = 7,
	[Obsolete]
	ChallengeMinor = 8,
	[Obsolete]
	ChallengeMajor = 9
}
