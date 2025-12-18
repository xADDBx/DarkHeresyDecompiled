using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("6b4b35d97564c3a4f900256bf0200a09")]
public class StarshipAddFeaturesOnSummon : BlueprintComponent
{
	[SerializeField]
	private bool ReverseLogicApplyToAllExcept;

	[SerializeField]
	private BlueprintStarship.Reference[] m_Blueprints = new BlueprintStarship.Reference[0];

	[SerializeField]
	private BlueprintFeatureReference[] m_Features = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintStarship> Blueprints
	{
		get
		{
			BlueprintReference<BlueprintStarship>[] blueprints = m_Blueprints;
			return blueprints;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}
}
