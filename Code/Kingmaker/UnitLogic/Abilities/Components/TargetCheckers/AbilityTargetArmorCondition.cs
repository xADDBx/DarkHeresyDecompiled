using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Target Restriction/AbilityTargetArmorCondition")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("c35fec6da99242e69db5fba70dbfdbd0")]
public class AbilityTargetArmorCondition : BlueprintComponent, IAbilityTargetRestriction
{
	public enum ConditionType
	{
		IsArmorFull = 1
	}

	public ConditionType CheckingType;

	public bool Inverted;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity != null)
		{
			PartArmor armorOptional = entity.GetArmorOptional();
			if (armorOptional != null)
			{
				if (CheckingType == ConditionType.IsArmorFull)
				{
					if (armorOptional.DurabilityLeft == armorOptional.DurabilityValue)
					{
						return !Inverted;
					}
					return Inverted;
				}
				return false;
			}
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return ConfigRoot.Instance.LocalizedTexts.Reasons.TargetArmorCondition;
	}
}
