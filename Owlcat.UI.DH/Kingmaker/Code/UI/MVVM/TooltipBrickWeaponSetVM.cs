using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWeaponSetVM : TooltipBaseBrickVM
{
	public HandSlot HandSlot;

	public bool IsPrimary;

	public ItemEntityWeapon Weapon;

	public EquipSlotVM EquipSlot;

	public readonly AutoDisposingList<CharInfoWeaponSetAbilityVM> Abilities = new AutoDisposingList<CharInfoWeaponSetAbilityVM>();

	public TooltipBrickWeaponSetVM(HandSlot handSlot, bool isPrimary)
	{
		HandSlot = handSlot;
		IsPrimary = isPrimary;
		AddDisposable(EquipSlot = new EquipSlotVM(EquipSlotType.PrimaryHand, handSlot));
		Weapon = (isPrimary ? HandSlot.HandsEquipmentSet.PrimaryHand.Weapon : HandSlot.HandsEquipmentSet.SecondaryHand.Weapon);
		foreach (WeaponAbility weaponAbility in Weapon.Blueprint.WeaponAbilities)
		{
			CharInfoWeaponSetAbilityVM charInfoWeaponSetAbilityVM = new CharInfoWeaponSetAbilityVM(weaponAbility.Ability, Weapon, Weapon.Owner);
			AddDisposable(charInfoWeaponSetAbilityVM);
			Abilities.Add(charInfoWeaponSetAbilityVM);
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Abilities.Clear();
	}
}
