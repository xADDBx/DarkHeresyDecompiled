using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityWeaponDamageVM : TooltipBrickVM
{
	public readonly Sprite WeaponIcon;

	public readonly string WeaponFamily;

	public readonly int BaseDamageMin;

	public readonly int BaseDamageMax;

	public readonly string DamageDescriptionText;

	public readonly IReadOnlyList<SpecialWeaponTagVM> WeaponTags;

	public BrickAbilityWeaponDamageVM(BlueprintItemWeapon blueprint, int baseDamageMin, int baseDamageMax, [CanBeNull] ItemEntityWeapon weapon = null)
	{
		WeaponIcon = blueprint.Icon;
		WeaponFamily = UIStrings.Instance.WeaponCategories.GetWeaponFamilyLabel(blueprint.Family);
		BaseDamageMin = baseDamageMin;
		BaseDamageMax = baseDamageMax;
		WeaponTags = GetRelevantTags(blueprint, weapon);
		DamageDescriptionText = UIStrings.Instance.Tooltips.Damage;
	}

	private IReadOnlyList<SpecialWeaponTagVM> GetRelevantTags(BlueprintItemWeapon blueprint, [CanBeNull] ItemEntityWeapon weapon)
	{
		List<SpecialWeaponTagVM> list = new List<SpecialWeaponTagVM>();
		foreach (var (type, value) in (weapon != null) ? UIUtilityItem.GetSpecialDamageValues(weapon) : UIUtilityItem.GetSpecialDamageValues(blueprint))
		{
			list.Add(new SpecialWeaponTagVM(type, value));
		}
		return list;
	}
}
