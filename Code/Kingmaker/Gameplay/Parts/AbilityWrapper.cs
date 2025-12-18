using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.Gameplay.Parts;

internal readonly struct AbilityWrapper
{
	[CanBeNull]
	public readonly Ability Ability;

	[CanBeNull]
	public readonly ToggleAbility ToggleAbility;

	public BlueprintMechanicEntityFact Blueprint
	{
		get
		{
			Ability ability = Ability;
			object obj = ((ability != null) ? SimpleBlueprintExtendAsObject.Or(ability.Blueprint, null) : null);
			if (obj == null)
			{
				ToggleAbility toggleAbility = ToggleAbility;
				if (toggleAbility == null)
				{
					return null;
				}
				obj = SimpleBlueprintExtendAsObject.Or(toggleAbility.Blueprint, null);
			}
			return (BlueprintMechanicEntityFact)obj;
		}
	}

	public AbilityWrapper([NotNull] Ability ability)
	{
		this = default(AbilityWrapper);
		Ability = ability;
	}

	public AbilityWrapper([NotNull] ToggleAbility toggleAbility)
	{
		this = default(AbilityWrapper);
		ToggleAbility = toggleAbility;
	}

	public MechanicActionBarSlot CreateSlot(BaseUnitEntity unit)
	{
		if (Ability != null)
		{
			return new MechanicActionBarSlotAbility
			{
				Ability = Ability.Data,
				Unit = unit,
				Replacement = unit.GetOptional<PartAbilityReplacements>()?.Get(Ability.Blueprint)
			};
		}
		if (ToggleAbility != null)
		{
			return new MechanicActionBarSlotToggleAbility
			{
				Ability = ToggleAbility,
				Unit = unit
			};
		}
		throw new Exception("AbilityWrapper.CreateSlot: invalid state, all variants are null");
	}
}
