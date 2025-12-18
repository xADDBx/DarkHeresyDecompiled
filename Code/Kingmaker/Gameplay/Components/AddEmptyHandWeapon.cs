using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Gameplay.Features.Items.Parts;
using Kingmaker.UnitLogic;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a80de4380faa4f6ead4b9209009a5d4d")]
public sealed class AddEmptyHandWeapon : UnitFactComponentDelegate
{
	public enum WeaponSetType
	{
		Both,
		First,
		Second
	}

	public enum HandType
	{
		Both,
		Primary,
		Secondary
	}

	public WeaponSetType WeaponSet;

	public HandType Hand;

	[ValidateNotNull]
	public BpRef<BlueprintItemWeapon> Weapon;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<PartEmptyHandWeapons>().Add(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartEmptyHandWeapons>()?.Remove(base.Fact, this);
	}
}
