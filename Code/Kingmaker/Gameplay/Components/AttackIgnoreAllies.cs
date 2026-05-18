using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("Excludes allies from valid targets for abilities matching the restriction. Works at prediction time (before ability execution).")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Ability/AttackIgnoreAllies")]
[TypeId("fa7f3deffb394514ab2f936d60e40bec")]
public sealed class AttackIgnoreAllies : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAttackIgnoreAllies>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAttackIgnoreAllies>()?.Unregister(base.Fact, this);
	}
}
