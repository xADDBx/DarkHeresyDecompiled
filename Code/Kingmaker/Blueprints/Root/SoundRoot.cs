using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Enums;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/Sound/SoundRoot")]
[TypeId("20e38815724b4954298551351a24f591")]
public class SoundRoot : BlueprintScriptableObject
{
	[Serializable]
	public class SoundEventByCrowdType
	{
		public GPUSoundController.CrowdType CrowdType;

		public AkSwitchReference switchSettings;

		public SoundEmitterBySizeCollection soundEmitterCollection;
	}

	[Serializable]
	public class SoundEmitterBySizeCollection
	{
		public SoundFx EmitterForSmall;

		public SoundFx EmitterForMedium;

		public SoundFx EmitterForLarge;
	}

	[AkEventReference]
	public string FinishingBlow;

	[Range(0f, 1f)]
	public float TBMIdleAudioOverride;

	public float AggroBarkRadius = 10f;

	public float LowHealthBarkHPPercent = 0.2f;

	public float LowShieldBarkPercent = 0.3f;

	public int EnemyMassDeathKillsCount = 3;

	public int TilesToBarkMoveOrderSpaceCombat = 4;

	public Size[] EnemyShipSizesToBarkEnemyDeathSC = new Size[3]
	{
		Size.Frigate_1x2,
		Size.Cruiser_2x4,
		Size.GrandCruiser_3x6
	};

	public Size[] EnemyShipSizesToBarkShieldIsDownSC = new Size[3]
	{
		Size.Frigate_1x2,
		Size.Cruiser_2x4,
		Size.GrandCruiser_3x6
	};

	public float StarSystemAudioScalingFactor = 1f;

	[Header("GPUCrowd Sound Settings")]
	public float MergeDistance;

	public SoundEventByCrowdType[] SoundEventsForCrowdTypes;

	public AkSwitchReference SmallCrowdSwitchSetting;

	public int MinMediumCrowdSize;

	public AkSwitchReference MediumCrowdSwitchSetting;

	public int MinHugeCrowdSize;

	public AkSwitchReference HugeCrowdSwitchSetting;

	public SoundFx TemplateSoundEventsEmitter;
}
