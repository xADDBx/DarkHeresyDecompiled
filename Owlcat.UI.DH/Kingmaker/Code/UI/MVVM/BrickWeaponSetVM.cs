using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWeaponSetVM : TooltipBrickVM
{
	public readonly HandSlot HandSlot;

	public readonly ItemEntityWeapon Weapon;

	public readonly EquipSlotVM EquipSlot;

	public readonly AutoDisposingList<CharInfoWeaponSetAbilityVM> Abilities = new AutoDisposingList<CharInfoWeaponSetAbilityVM>();

	public BrickWeaponSetVM(HandSlot handSlot, bool isPrimary)
	{
		HandSlot = handSlot;
		EquipSlot = new EquipSlotVM(EquipSlotType.PrimaryHand, handSlot).AddTo(this);
		Weapon = (isPrimary ? HandSlot.HandsEquipmentSet.PrimaryHand.Weapon : HandSlot.HandsEquipmentSet.SecondaryHand.Weapon);
		foreach (WeaponAbility weaponAbility in Weapon.Blueprint.WeaponAbilities)
		{
			CharInfoWeaponSetAbilityVM item = new CharInfoWeaponSetAbilityVM(weaponAbility.Ability, Weapon, Weapon.Owner).AddTo(this);
			Abilities.Add(item);
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Abilities.Clear();
	}
}
