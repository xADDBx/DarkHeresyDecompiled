using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("80356eece01ff7949a0c0191afdf7a3a")]
public class EnableAttackWithPairedWeapon : UnitFactComponentDelegate, IEntitySubscriber
{
	protected override void OnActivateOrPostLoad()
	{
		SwitchAttackWithPairedWeapon(value: true);
	}

	protected override void OnDeactivate()
	{
		SwitchAttackWithPairedWeapon(value: false);
	}

	private void SwitchAttackWithPairedWeapon(bool value)
	{
		PartTwoWeaponFighting twoWeaponFightingOptional = base.Owner.GetTwoWeaponFightingOptional();
		if (twoWeaponFightingOptional != null)
		{
			twoWeaponFightingOptional.EnableAttackWithPairedWeapon = value;
		}
	}

	private void UpdateAbilityWeaponGroups()
	{
		BlueprintCombatRoot combatRoot = ConfigRoot.Instance.CombatRoot;
		HandsEquipmentSet handsEquipmentSet = base.Owner.GetBodyOptional()?.CurrentHandsEquipmentSet;
		foreach (Ability rawFact in base.Owner.Abilities.RawFacts)
		{
			ItemEntityWeapon sourceWeapon = rawFact.Data.SourceWeapon;
			if (sourceWeapon != null && !sourceWeapon.HoldInTwoHands)
			{
				if (sourceWeapon.HoldingSlot == handsEquipmentSet?.PrimaryHand)
				{
					rawFact.Data.AbilityGroups.Remove(combatRoot.SecondaryHandAbilityGroup);
				}
				if (sourceWeapon.HoldingSlot == handsEquipmentSet?.SecondaryHand)
				{
					rawFact.Data.AbilityGroups.Remove(combatRoot.PrimaryHandAbilityGroup);
				}
			}
			if (rawFact.Blueprint.IsWeaponAbility && rawFact.Blueprint.Type == AbilityType.Spell && !rawFact.Blueprint.IsFreeAction)
			{
				rawFact.Data.AbilityGroups.Remove(combatRoot.SecondaryHandAbilityGroup);
			}
		}
	}
}
