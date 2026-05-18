using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Customization;

[TypeId("2c948c91f467b544b93a831659858496")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintRaceAppearance : BlueprintScriptableObject
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Presets")]
	private BlueprintRaceVisualPresetReference[] m_Presets = new BlueprintRaceVisualPresetReference[0];

	[NotNull]
	public CustomizationOptions MaleOptions = new CustomizationOptions();

	[NotNull]
	public CustomizationOptions FemaleOptions = new CustomizationOptions();

	public ReferenceArrayProxy<BlueprintRaceVisualPreset> Presets
	{
		get
		{
			BlueprintReference<BlueprintRaceVisualPreset>[] presets = m_Presets;
			return presets;
		}
	}
}
