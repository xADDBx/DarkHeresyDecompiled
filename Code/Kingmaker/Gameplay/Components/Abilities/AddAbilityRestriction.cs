using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Components.Abilities;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Ability/AddAbilityRestriction")]
[TypeId("7fe13da87f9444738729b9a1f7495b65")]
public sealed class AddAbilityRestriction : UnitFactComponentDelegate
{
	[Tooltip("Если true, то все абилки подходящие под фильтр будут недоступны")]
	public bool ForbidAbility;

	public RestrictionCalculator AbilityFilter;

	[HideIf("ForbidAbility")]
	[Tooltip("CurrentEntity - кастер, CurrentTarget - цель для проверки")]
	public RestrictionCalculator TargetRestriction;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityRestrictions>().AddEntry(base.Fact, this, AbilityFilter, TargetRestriction, ForbidAbility);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityRestrictions>()?.RemoveEntry(base.Fact, this);
	}
}
