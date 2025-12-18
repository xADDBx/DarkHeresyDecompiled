using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponSetVM : ViewModel
{
	private readonly ReactiveProperty<EquipSlotVM> m_SelectedHand = new ReactiveProperty<EquipSlotVM>();

	public readonly ObservableList<CharInfoWeaponSetAbilityVM> Abilities = new ObservableList<CharInfoWeaponSetAbilityVM>();

	private MechanicEntity m_Caster;

	public EquipSlotVM Primary { get; set; }

	public EquipSlotVM Secondary { get; set; }

	public ReadOnlyReactiveProperty<EquipSlotVM> SelectedHand => m_SelectedHand;

	public CharInfoWeaponSetVM(HandsEquipmentSet equipmentSet, MechanicEntity caster)
	{
		m_Caster = caster;
		Primary = new EquipSlotVM(EquipSlotType.PrimaryHand, equipmentSet.PrimaryHand).AddTo(this);
		Secondary = new EquipSlotVM(EquipSlotType.SecondaryHand, equipmentSet.SecondaryHand, -1, Primary).AddTo(this);
		SelectedHand.Subscribe(delegate
		{
			UpdateAbilities();
		}).AddTo(this);
		UpdateSelectedHand();
	}

	public void SelectHand(EquipSlotVM slotVm)
	{
		if (slotVm != null)
		{
			m_SelectedHand.Value = slotVm;
		}
	}

	private void UpdateSelectedHand()
	{
		if (Primary.ItemSlot.HasItem)
		{
			m_SelectedHand.Value = Primary;
		}
		else
		{
			m_SelectedHand.Value = (Secondary.ItemSlot.HasItem ? Secondary : null);
		}
	}

	private void UpdateAbilities()
	{
		Abilities.Clear();
		if (!(SelectedHand.CurrentValue?.Item.CurrentValue is ItemEntityWeapon itemEntityWeapon))
		{
			return;
		}
		foreach (WeaponAbility weaponAbility in itemEntityWeapon.Blueprint.WeaponAbilities)
		{
			CharInfoWeaponSetAbilityVM item = new CharInfoWeaponSetAbilityVM(weaponAbility.Ability, itemEntityWeapon, m_Caster).AddTo(this);
			Abilities.Add(item);
		}
	}
}
