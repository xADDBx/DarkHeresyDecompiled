using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.View.MapObjects;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public static class MechanicEntityHelper
{
	public static readonly Comparison<Entity> ByIdComparison = (Entity a, Entity b) => string.Compare(a.UniqueId, b.UniqueId, StringComparison.Ordinal);

	[CanBeNull]
	public static ILootable GetLoot(this MechanicEntity entity)
	{
		if (!entity.GetOptional<InteractionLootPart>())
		{
			return null;
		}
		return entity as ILootable;
	}

	public static AbilityData GetAbilityData(this MechanicEntity caster, BlueprintAbility ability)
	{
		return caster.Facts.Get<Ability>(ability)?.Data;
	}

	public static AbilityData GetAbilityDataWithUpgrades(this MechanicEntity caster, BlueprintAbility ability)
	{
		return ability.Upgrades.Select((BpRef<BlueprintAbility> u) => caster.GetAbilityData(u)).FirstOrDefault((AbilityData a) => a != null) ?? caster.GetAbilityData(ability);
	}

	public static Vector3 GetDesiredPosition(this MechanicEntity entity)
	{
		return Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(entity);
	}
}
