using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityUnit
{
	private static EnumUnitSubtypeIcons SubtypeIcons => UIConfig.Instance.Portraits.UnitSubtypeIcons;

	private static EnumUnitSubtypeIcons SubtypePortrait => UIConfig.Instance.Portraits.UnitSubtypePortrait;

	public static IEnumerable<Ability> CollectAbilities(BaseUnitEntity unit)
	{
		return unit.Abilities.Visible.Where((Ability a) => a.SourceItem == null);
	}

	public static IEnumerable<TBlueprint> GetBlueprintUnitFactFromFact<TBlueprint>(BlueprintMechanicEntityFact blueprintMechanicEntityFact) where TBlueprint : BlueprintUnitFact
	{
		ReferenceArrayProxy<BlueprintUnitFact>? referenceArrayProxy = blueprintMechanicEntityFact.GetComponent<AddFacts>()?.Facts;
		IEnumerable<TBlueprint> enumerable = (referenceArrayProxy.HasValue ? referenceArrayProxy.GetValueOrDefault().OfType<TBlueprint>() : null);
		if (blueprintMechanicEntityFact.GetComponent<AddFact>()?.Fact is TBlueprint element)
		{
			(enumerable ?? Array.Empty<TBlueprint>()).Append(element);
		}
		return enumerable;
	}

	public static StatType? GetSourceStatType(ModifiableValue stat)
	{
		if (stat == null)
		{
			return null;
		}
		if (stat is ModifiableValueSkill modifiableValueSkill)
		{
			return modifiableValueSkill.BaseStat.Type;
		}
		return null;
	}

	public static int GetSurfaceEnemyDifficulty([CanBeNull] BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return 0;
		}
		return (int)(unit.Blueprint.DifficultyType + 1);
	}

	public static bool UsedSubtypeIcon(MechanicEntity mechanicEntityEntity)
	{
		return mechanicEntityEntity?.GetUnitUISettingsOptional()?.Portrait.SmallPortrait == null;
	}

	public static Sprite GetSurfaceCombatStandardPortrait(MechanicEntity mechanicEntityEntity, PortraitCombatSize size)
	{
		PortraitData portraitData = mechanicEntityEntity?.GetUnitUISettingsOptional()?.Portrait;
		UnitSubtype val = (mechanicEntityEntity as UnitEntity)?.Blueprint.Subtype ?? UnitSubtype.Default;
		return size switch
		{
			PortraitCombatSize.Icon => portraitData?.SmallPortrait ?? SubtypeIcons.GetSprite(val), 
			PortraitCombatSize.Small => portraitData?.SmallPortrait ?? SubtypePortrait.GetSprite(val), 
			PortraitCombatSize.Middle => portraitData?.HalfLengthPortrait ?? SubtypePortrait.GetSprite(val), 
			_ => portraitData?.SmallPortrait ?? SubtypePortrait.GetSprite(val), 
		};
	}

	public static bool IsCastingAbility([CanBeNull] this BaseUnitEntity unit)
	{
		return unit?.Commands?.Current is UnitUseAbility;
	}

	public static bool InPartyAndControllable(this MechanicEntity unit)
	{
		PartFaction factionOptional = unit.GetFactionOptional();
		if ((object)factionOptional != null && factionOptional.IsDirectlyControllable)
		{
			return unit.CanBeControlled();
		}
		return false;
	}

	public static StatType? TryGetStatType(string key)
	{
		try
		{
			return (StatType)Enum.Parse(typeof(StatType), key);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static bool IsViewActiveUnit(BaseUnitEntity unit)
	{
		return unit.IsViewActive;
	}
}
