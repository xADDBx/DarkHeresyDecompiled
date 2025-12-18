using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.View.Animation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Progression.Features;

[TypeId("9a3d49fbb9d4c174d8ec1090679fe3e3")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintRace : BlueprintFeature
{
	public string SoundKey = "";

	public Race RaceId = Race.Unknown;

	public bool SelectableRaceStat;

	public Size Size = Size.Medium;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Features")]
	private BlueprintFeatureBaseReference[] m_Features = new BlueprintFeatureBaseReference[0];

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Presets")]
	private BlueprintRaceVisualPresetReference[] m_Presets = new BlueprintRaceVisualPresetReference[0];

	[NotNull]
	public CustomizationOptions MaleOptions = new CustomizationOptions();

	[NotNull]
	public CustomizationOptions FemaleOptions = new CustomizationOptions();

	[NotNull]
	public UnitAnimationSettings MaleSpeedSettings;

	[NotNull]
	public UnitAnimationSettings FemaleSpeedSettings;

	public ReferenceArrayProxy<BlueprintFeatureBase> Features
	{
		get
		{
			BlueprintReference<BlueprintFeatureBase>[] features = m_Features;
			return features;
		}
	}

	public ReferenceArrayProxy<BlueprintRaceVisualPreset> Presets
	{
		get
		{
			BlueprintReference<BlueprintRaceVisualPreset>[] presets = m_Presets;
			return presets;
		}
	}

	public UnitAnimationSettings GetSpeedSettings(Gender gender)
	{
		return gender switch
		{
			Gender.Male => MaleSpeedSettings, 
			Gender.Female => FemaleSpeedSettings, 
			_ => throw new ArgumentOutOfRangeException("gender", gender, null), 
		};
	}

	[CanBeNull]
	public EquipmentEntityLink GetTail(Gender gender, int skinRampIndex)
	{
		if (gender != 0)
		{
			return FemaleOptions.GetTail(skinRampIndex);
		}
		return MaleOptions.GetTail(skinRampIndex);
	}
}
