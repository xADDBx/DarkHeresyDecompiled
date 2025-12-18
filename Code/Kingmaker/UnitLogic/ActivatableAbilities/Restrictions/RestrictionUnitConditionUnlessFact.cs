using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[Obsolete]
[AllowMultipleComponents]
[ComponentName("AA restriction unit condition")]
[TypeId("27de986d733ccd9498bca34f20529fe7")]
public class RestrictionUnitConditionUnlessFact : ActivatableAbilityRestriction
{
	public UnitCondition Condition;

	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintUnitFactReference m_CheckedFact;

	public BlueprintUnitFact CheckedFact => m_CheckedFact?.Get();

	protected override bool IsAvailable()
	{
		return false;
	}
}
