using JetBrains.Annotations;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Sound;
using Kingmaker.Visual.Animation.CustomIdleComponents;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IUnitSpawnerConfig : IAbstractUnitSpawnerConfig, IEntityConfig
{
	[CanBeNull]
	BlueprintEncounter Encounter { get; }

	VoIdIndex VoIdIndex { get; }

	bool IsLightweight { get; }

	bool BossMusicEnable { get; }

	AkStateReference MusicBossFightType { get; }

	[CanBeNull]
	CustomIdleAnimationMonoComponent CustomIdleAnimation { get; }
}
