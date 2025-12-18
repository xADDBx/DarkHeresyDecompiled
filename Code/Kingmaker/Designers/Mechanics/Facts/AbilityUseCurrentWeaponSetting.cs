using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("256c41efd74f4792a30353c4cf1cc1b2")]
public sealed class AbilityUseCurrentWeaponSetting : MechanicEntityFactComponentDelegate, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IUnitActiveEquipmentSetHandler<EntitySubscriber>, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitActiveEquipmentSetHandler, EntitySubscriber>
{
	public bool useSecondWeapon;

	void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		UpdateWeapon();
	}

	void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet()
	{
		UpdateWeapon();
	}

	private void UpdateWeapon()
	{
		if (base.Fact is Ability ability)
		{
			Set(ability.Data);
		}
	}

	public void Set(AbilityData ability)
	{
		ItemEntityWeapon itemEntityWeapon = ((!useSecondWeapon) ? ability.Caster.GetFirstWeapon() : ability.Caster.GetSecondWeapon());
		if (ability.OverrideWeapon == itemEntityWeapon)
		{
			return;
		}
		ability.OverrideWeapon = itemEntityWeapon;
		object fXSettingsOverride;
		if (itemEntityWeapon != null)
		{
			BlueprintAbilityFXSettings bestFXSettings = GetBestFXSettings(itemEntityWeapon, ability);
			if (bestFXSettings != null)
			{
				fXSettingsOverride = bestFXSettings;
				goto IL_0045;
			}
		}
		fXSettingsOverride = null;
		goto IL_0045;
		IL_0045:
		ability.FXSettingsOverride = (BlueprintAbilityFXSettings)fXSettingsOverride;
	}

	[CanBeNull]
	private static BlueprintAbilityFXSettings GetBestFXSettings(ItemEntityWeapon weapon, AbilityData ability)
	{
		AbilityData abilityData = null;
		foreach (Ability ability3 in weapon.Abilities)
		{
			BlueprintAbilityWrapper blueprint = ability3.Data.Blueprint;
			if (blueprint.AttackType == ability.Blueprint.AttackType)
			{
				return ability3.Data.FXSettings;
			}
			if (blueprint.AttackType.HasValue && (abilityData == null || (!abilityData.Blueprint.IsBurst && blueprint.IsBurst)))
			{
				abilityData = ability3.Data;
			}
		}
		object obj = abilityData?.FXSettings;
		if (obj == null)
		{
			Ability ability2 = weapon.Abilities.FirstItem();
			if (ability2 == null)
			{
				return null;
			}
			obj = ability2.Data.FXSettings;
		}
		return (BlueprintAbilityFXSettings)obj;
	}
}
