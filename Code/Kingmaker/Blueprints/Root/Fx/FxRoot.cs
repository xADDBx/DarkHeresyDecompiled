using System;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles.Blueprints;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Fx;

[ComponentName("Root/FxRoot")]
[TypeId("9e4194ae2db2427eb4cb1d891b8c8670")]
public class FxRoot : BlueprintScriptableObject
{
	[Serializable]
	public class FootprintSurfaceSettings
	{
		public SurfaceType GroundType;

		public Color FootprintTintColor;

		public float FootprintAlphaScale = 0.6f;

		public float FootprintBumpScale = 1f;

		public float FootprintRoughness = 1f;
	}

	[Header("Locator Groups")]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintFxLocatorGroup.Reference m_LocatorGroupMuzzle;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFxLocatorGroup.Reference m_LocatorGroupDefaultHit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFxLocatorGroup.Reference m_LocatorGroupTorso;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFxLocatorGroup.Reference m_LocatorGroupHitter;

	[SerializeField]
	[ValidateNotNull]
	[Tooltip("Used for EntityView CenterTorso reference")]
	private BlueprintFxLocatorGroup.Reference m_LocatorGroupTorsoCenterFX;

	public string[] FallEventStrings = new string[0];

	[AssetPicker("")]
	public GameObject DustOnFallPrefab;

	public DeathFxFromEnergyEntry[] OverrideDeathPrefabsFromEnergy;

	[Header("AnimatedLight time of day settings")]
	public float IntensityMultiplierMorning = 1f;

	public float IntensityMultiplierDay = 1f;

	public float IntensityMultiplierEvening = 1f;

	public float IntensityMultiplierNight = 1f;

	public float RangeMultiplierMorning = 1f;

	public float RangeMultiplierDay = 1f;

	public float RangeMultiplierEvening = 1f;

	public float RangeMultiplierNight = 1f;

	public RaceFxScaleSettings RaceFxSnapMapScaleSettings;

	public RaceFxScaleSettings RaceFxSnapToLocatorScaleSettings;

	[Header("Footprint")]
	public float DefaultLifetimeSeconds = 20f;

	public float FadeOutTimeSeconds = 1f;

	public int MaxFootprintsCountPerPlayerUnit = 20;

	[Tooltip("Percentage from player unit footprints count for NPC. 1 means 100%")]
	[Range(0f, 1f)]
	public float MaxFootprintsCountModForNPC = 0.5f;

	public float MinDistanceBetweenFootprints = 0.2f;

	public PrefabLink StubFootprint;

	public PrefabLink DefaultHumanFootprint;

	public PrefabLink DefaultEldarFootprint;

	public PrefabLink DefaultSpaceMarineFootprint;

	public PrefabLink DefaultKrootFootprint;

	public PrefabLink DefaultOgrynFootprint;

	public FootprintSurfaceSettings[] FootprintsSettings;

	private bool m_DeathFxsInitialized;

	private DeathFxFromEnergyEntry[] m_CachedOverrideDeathPrefabsFromEnergy;

	[Header("Misc")]
	public int MaxMaterialLayersCount = 2;

	[AkEventReference]
	public string InCoverHitSoundEvent;

	public PrefabLink FXAttackOfOpportunity;

	public PrefabLink JumpAsideDodgeFX;

	public PrefabLink SimpleDodgeFX;

	[Header("Hologram")]
	public PrefabLink DefaultHologramPrefab;

	public PrefabLink DefaultStarshipHologramPrefab;

	public HologramRaceFx[] HologramPrefabs;

	public HologramEntry Hologram;

	public BlueprintBuffReference PreparationTurnVisualBuff;

	[Header("Occluder Highlighter")]
	public Color OccluderColorDefault = Color.black;

	public Color OccluderColorAlly = Color.green;

	public Color OccluderColorEnemy = Color.red;

	public Color OccluderColorUnknownCombatant = Color.yellow;

	[Header("UI Glitch")]
	public SpriteGlitchSurfaceOvertipSettings SpriteGlitchSurfaceOvertipSettings;

	[Header("Body parts highlight on precise shot")]
	public PrefabLink BodyPartHighlightFx;

	[Header("Ability target highlight")]
	public PrefabLink AbilityTargetHighlightFx;

	public static FxRoot Instance => ConfigRoot.Instance.FxRoot;

	public BlueprintFxLocatorGroup LocatorGroupMuzzle => m_LocatorGroupMuzzle;

	public BlueprintFxLocatorGroup LocatorGroupDefaultHit => m_LocatorGroupDefaultHit;

	public BlueprintFxLocatorGroup LocatorGroupTorso => m_LocatorGroupTorso;

	public BlueprintFxLocatorGroup LocatorGroupHitter => m_LocatorGroupHitter;

	public BlueprintFxLocatorGroup LocatorGroupTorsoCenterFX => m_LocatorGroupTorsoCenterFX;

	public void Deinit()
	{
		m_DeathFxsInitialized = false;
	}

	private void InitializeDeathFxs()
	{
		if (!m_DeathFxsInitialized)
		{
			m_CachedOverrideDeathPrefabsFromEnergy = new DeathFxFromEnergyEntry[DeathFxFromEnergyEntry.GetMaxEntriesCount()];
			DeathFxFromEnergyEntry[] overrideDeathPrefabsFromEnergy = OverrideDeathPrefabsFromEnergy;
			foreach (DeathFxFromEnergyEntry deathFxFromEnergyEntry in overrideDeathPrefabsFromEnergy)
			{
				m_CachedOverrideDeathPrefabsFromEnergy[deathFxFromEnergyEntry.GetEntryIndex()] = deathFxFromEnergyEntry;
			}
			m_DeathFxsInitialized = true;
		}
	}

	public float GetLightIntensityMultiplier(Vector3 position)
	{
		return (Game.Instance.CurrentlyLoadedAreaPart?.GetTimeOfDay() ?? Game.Instance.TimeOfDay) switch
		{
			TimeOfDay.Morning => IntensityMultiplierMorning, 
			TimeOfDay.Day => IntensityMultiplierDay, 
			TimeOfDay.Evening => IntensityMultiplierEvening, 
			TimeOfDay.Night => IntensityMultiplierNight, 
			_ => 1f, 
		};
	}

	public float GetLightRangeMultiplier(Vector3 position)
	{
		return (Game.Instance.CurrentlyLoadedAreaPart?.GetTimeOfDay() ?? Game.Instance.TimeOfDay) switch
		{
			TimeOfDay.Morning => RangeMultiplierMorning, 
			TimeOfDay.Day => RangeMultiplierDay, 
			TimeOfDay.Evening => RangeMultiplierEvening, 
			TimeOfDay.Night => RangeMultiplierNight, 
			_ => 1f, 
		};
	}

	public DeathFxFromEnergyEntry DeathFxOptionForEnergyDamage(DamageType type)
	{
		InitializeDeathFxs();
		int energyDamageEntryIndex = DeathFxFromEnergyEntry.GetEnergyDamageEntryIndex(type);
		return m_CachedOverrideDeathPrefabsFromEnergy[energyDamageEntryIndex] ?? new DeathFxFromEnergyEntry();
	}
}
