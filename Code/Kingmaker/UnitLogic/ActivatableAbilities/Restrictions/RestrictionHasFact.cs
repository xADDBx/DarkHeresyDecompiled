using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[Obsolete]
[TypeId("74999d4fdbb7037469dcc37ac14a67f5")]
public class RestrictionHasFact : ActivatableAbilityRestriction
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintUnitFactReference m_Feature;

	public bool Not;

	public BlueprintUnitFact Feature => m_Feature?.Get();

	protected override bool IsAvailable()
	{
		if (!Not)
		{
			return base.Owner.Facts.Contains(Feature);
		}
		return !base.Owner.Facts.Contains(Feature);
	}
}
