using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Customization;
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

	[CanBeNull]
	[SerializeField]
	private BlueprintRaceAppearanceReference m_Appearance;

	public ReferenceArrayProxy<BlueprintFeatureBase> Features
	{
		get
		{
			BlueprintReference<BlueprintFeatureBase>[] features = m_Features;
			return features;
		}
	}

	[CanBeNull]
	public BlueprintRaceAppearance Appearance => m_Appearance?.Get();
}
