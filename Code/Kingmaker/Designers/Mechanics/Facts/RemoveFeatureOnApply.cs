using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("f46c5f588edca9e47b0dc0509fbcf591")]
public class RemoveFeatureOnApply : UnitFactComponentDelegate
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintUnitFactReference m_Feature;

	public BlueprintUnitFact Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		base.Owner.Facts.Remove(Feature);
	}
}
