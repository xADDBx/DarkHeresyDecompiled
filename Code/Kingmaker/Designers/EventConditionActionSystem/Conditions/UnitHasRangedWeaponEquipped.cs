using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("9ddee6a6ebce4240ab83fb2a6a5536ce")]
public class UnitHasRangedWeaponEquipped : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public WeaponCheckType CheckType;

	[InfoBox("Actually, checking current weapon set")]
	public bool CheckMainSlotOnly;

	public bool AnySlot = true;

	[HideIf("AnySlot")]
	public bool InPrimarySlot;

	[HideIf("AnySlot")]
	public bool InSecondarySlot;

	public override string GetDescription()
	{
		return "Выдает true, если в текущем сете у юнита есть " + CheckType.ToString() + " оружие";
	}

	protected override string GetConditionCaption()
	{
		return $"Returns true if {Unit} has a {CheckType.ToString()} weapon equipped";
	}

	protected override bool CheckCondition()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return false;
		}
		if (CheckMainSlotOnly)
		{
			if (AnySlot)
			{
				if (IsWeaponSuitable(baseUnitEntity.Body.PrimaryHand.MaybeWeapon))
				{
					return true;
				}
				if (IsWeaponSuitable(baseUnitEntity.Body.SecondaryHand.MaybeWeapon))
				{
					return true;
				}
				if (baseUnitEntity.Body.AdditionalLimbs.Any((WeaponSlot x) => IsWeaponSuitable(x.MaybeWeapon)))
				{
					return true;
				}
			}
			else
			{
				if (InPrimarySlot && IsWeaponSuitable(baseUnitEntity.Body.PrimaryHand.MaybeWeapon))
				{
					return true;
				}
				if (InSecondarySlot && IsWeaponSuitable(baseUnitEntity.Body.SecondaryHand.MaybeWeapon))
				{
					return true;
				}
			}
			return false;
		}
		foreach (HandsEquipmentSet handsEquipmentSet in baseUnitEntity.Body.HandsEquipmentSets)
		{
			if (!handsEquipmentSet.IsEmpty() && (((AnySlot || InPrimarySlot) && IsWeaponSuitable(handsEquipmentSet.PrimaryHand.MaybeWeapon)) || ((AnySlot || InSecondarySlot) && IsWeaponSuitable(handsEquipmentSet.SecondaryHand.MaybeWeapon))))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWeaponSuitable([CanBeNull] ItemEntityWeapon weapon)
	{
		if (weapon == null)
		{
			return false;
		}
		return CheckType switch
		{
			WeaponCheckType.Any => true, 
			WeaponCheckType.Melee => weapon.Blueprint.IsMelee, 
			WeaponCheckType.Ranged => weapon.Blueprint.IsRanged, 
			_ => false, 
		};
	}
}
