using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("e054742513e17a4459e8299b6a2e949d")]
public class CurrentWeaponFactionGetter : BoolPropertyGetter
{
	public enum WeaponHand
	{
		MainHand,
		OffHand
	}

	public ItemFaction Faction;

	public WeaponHand Hand;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Current {Hand} weapon faction == {Faction}";
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = ((Hand == WeaponHand.OffHand) ? baseUnitEntity.Body.SecondaryHand.MaybeWeapon : baseUnitEntity.Body.PrimaryHand.MaybeWeapon);
		if (itemEntityWeapon != null)
		{
			return itemEntityWeapon.EffectiveFaction == Faction;
		}
		return false;
	}
}
