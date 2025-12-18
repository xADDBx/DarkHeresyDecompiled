using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintPath))]
[ComponentName("Facts And Buffs/AddFeaturesToLevelUp")]
[TypeId("00cc9fe0a3f84bc1811764509b4282aa")]
public class AddFeaturesToLevelUp : BlueprintComponent
{
	public FeatureGroup Group;

	[SerializeField]
	private BlueprintFeatureReference[] m_Features = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}
}
