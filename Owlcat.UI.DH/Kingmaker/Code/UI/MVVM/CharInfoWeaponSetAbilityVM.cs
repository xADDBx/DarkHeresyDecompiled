using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponSetAbilityVM : ViewModel
{
	private readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>(null);

	private readonly ReactiveProperty<BlueprintAbility> m_Ability = new ReactiveProperty<BlueprintAbility>(null);

	private readonly ReactiveProperty<ItemEntityWeapon> m_ItemWeapon = new ReactiveProperty<ItemEntityWeapon>(null);

	public readonly List<CharInfoAbilityStatVM> Stats = new List<CharInfoAbilityStatVM>();

	private readonly MechanicEntity m_Caster;

	public ReadOnlyReactiveProperty<Sprite> Icon => m_Icon;

	public ReadOnlyReactiveProperty<BlueprintAbility> Ability => m_Ability;

	public ReadOnlyReactiveProperty<ItemEntityWeapon> ItemWeapon => m_ItemWeapon;

	public TooltipTemplateAbility Tooltip { get; private set; }

	public CharInfoWeaponSetAbilityVM(BlueprintAbility ability, ItemEntityWeapon weapon, MechanicEntity caster)
	{
		m_Caster = caster;
		m_Icon.Value = ability.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		Tooltip = new TooltipTemplateAbility(ability, weapon.Blueprint, caster);
		m_Ability.Value = ability;
		m_ItemWeapon.Value = weapon;
		CreateItemAbilityStats();
	}

	public void CreateItemAbilityStats()
	{
		if (Ability.CurrentValue != null && ItemWeapon.CurrentValue != null)
		{
			if (UIUtilityItem.CreateAbilityData(Ability.CurrentValue, ItemWeapon.CurrentValue, m_Caster).IsPrecise)
			{
				TryCreateToList(WeaponStat.PreciseHitChance);
				TryCreateToList(WeaponStat.AdditionalVitalDamage);
				TryCreateToList(WeaponStat.AmmoCount);
			}
			else if (ItemWeapon.CurrentValue.Blueprint.IsRanged)
			{
				TryCreateToList(WeaponStat.ShotHitChance);
				TryCreateToList(WeaponStat.AdditionalArmorDamage);
				TryCreateToList(WeaponStat.AdditionalWoundsDamage);
				TryCreateToList(WeaponStat.AdditionalVitalDamage);
				TryCreateToList(WeaponStat.AmmoCount);
			}
			else if (ItemWeapon.CurrentValue.Blueprint.IsMelee)
			{
				TryCreateToList(WeaponStat.StrikeHitChance);
				TryCreateToList(WeaponStat.AdditionalArmorDamage);
				TryCreateToList(WeaponStat.AdditionalWoundsDamage);
				TryCreateToList(WeaponStat.AdditionalVitalDamage);
			}
			else
			{
				TryCreateToList(WeaponStat.ShotHitChance);
			}
		}
	}

	private void TryCreateToList(WeaponStat stat)
	{
		CharInfoAbilityStatVM charInfoAbilityStatVM = CharInfoAbilityStatVMFactory.TryCreate(stat, Ability.CurrentValue, ItemWeapon.CurrentValue, m_Caster);
		if (charInfoAbilityStatVM != null)
		{
			charInfoAbilityStatVM.AddTo(this);
			Stats.Add(charInfoAbilityStatVM);
		}
	}
}
