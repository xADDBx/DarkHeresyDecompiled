using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("AoO is allowed when AllowingCondition passes. Empty condition = always allowed. Context: Caster = who applied the buff, Target = AoO provoker, Owner = the restricted unit.")]
[ComponentName("Combat/AoORestriction")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("c580335cf5c42ec4daed84df480db72e")]
public sealed class AoORestriction : UnitFactComponentDelegate
{
	public RestrictionCalculator AllowingCondition = new RestrictionCalculator();

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartAoOInstructions>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartAoOInstructions>()?.Unregister(base.Fact, this);
	}
}
