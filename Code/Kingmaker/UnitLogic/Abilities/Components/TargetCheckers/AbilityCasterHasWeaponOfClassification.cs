using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowMultipleComponents]
[ComponentName("Caster Restriction/AbilityCasterHasWeaponOfClassification")]
[TypeId("68b70115a9624633af295d7e88c188ab")]
public class AbilityCasterHasWeaponOfClassification : BlueprintComponent, IAbilityCasterRestriction
{
	public WeaponClassification Classification;

	public bool CheckCurrentSet;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool flag = false;
		IList<HandsEquipmentSet> list = caster.GetBodyOptional()?.HandsEquipmentSets;
		if (list == null)
		{
			return false;
		}
		if (!CheckCurrentSet)
		{
			foreach (HandsEquipmentSet item in list)
			{
				flag |= (item.PrimaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon blueprintItemWeapon && blueprintItemWeapon.Classification == Classification) || (item.SecondaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon blueprintItemWeapon2 && blueprintItemWeapon2.Classification == Classification);
			}
			return flag;
		}
		return flag | ((caster.GetBodyOptional()?.PrimaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon blueprintItemWeapon3 && blueprintItemWeapon3.Classification == Classification) || (caster.GetBodyOptional()?.SecondaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon blueprintItemWeapon4 && blueprintItemWeapon4.Classification == Classification));
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return ConfigRoot.Instance.LocalizedTexts.Reasons.CasterHasNoWeaponOfClassification;
	}
}
