using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("800c686eb72c8b24ab4c8bb6ae54c065")]
public class SavesFixerReplaceInProgression : UnitFactComponentDelegate
{
	[SerializeField]
	[FormerlySerializedAs("OldFeature")]
	private BlueprintFeatureReference m_OldFeature;

	[SerializeField]
	[FormerlySerializedAs("NewFeature")]
	private BlueprintFeatureReference m_NewFeature;

	public BlueprintFeature OldFeature => m_OldFeature?.Get();

	public BlueprintFeature NewFeature => m_NewFeature?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Facts.Contains(OldFeature))
		{
			base.Owner.Progression.ReplaceFeature(OldFeature, NewFeature);
		}
	}
}
