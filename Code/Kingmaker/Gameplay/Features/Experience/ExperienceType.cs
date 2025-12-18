using System;
using UnityEngine.Serialization;

namespace Kingmaker.Gameplay.Features.Experience;

public enum ExperienceType
{
	[FormerlySerializedAs("Mob")]
	Encounter,
	SkillCheck,
	Quest,
	MainQuest,
	Investigation,
	[Obsolete]
	Boss,
	[Obsolete]
	QuestMain,
	[Obsolete]
	QuestNormal,
	[Obsolete]
	ChallengeMinor,
	[Obsolete]
	ChallengeMajor
}
