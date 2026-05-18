using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Interaction;
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

	public static StatType? GetSourceStatType(StatType statType)
	{
		return MechanicActor.GetStatBaseStat(statType);
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
		if (!(mechanicEntityEntity is BaseUnitEntity baseUnitEntity))
		{
			return true;
		}
		PartUnitUISettings unitUISettingsOptional = baseUnitEntity.GetUnitUISettingsOptional();
		if ((unitUISettingsOptional != null && (unitUISettingsOptional.CustomPortraitRaw != null || unitUISettingsOptional.PortraitBlueprintRaw != null)) ? true : false)
		{
			return false;
		}
		return !baseUnitEntity.Blueprint.HasCustomPortrait;
	}

	public static Sprite GetSurfaceCombatStandardPortrait(MechanicEntity mechanicEntityEntity, PortraitCombatSize size)
	{
		PortraitData portraitData = (UsedSubtypeIcon(mechanicEntityEntity) ? null : mechanicEntityEntity?.GetUnitUISettingsOptional()?.Portrait);
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

	public static IUnitInteraction GetUnitInteractionFrom(MechanicEntity entity)
	{
		if (entity == null || entity.IsDisposed)
		{
			return null;
		}
		TurnController turnController = Game.Instance.Controllers.TurnController;
		BaseUnitEntity baseUnitEntity2;
		if (turnController != null)
		{
			MechanicEntity currentUnit = turnController.CurrentUnit;
			if (currentUnit is BaseUnitEntity baseUnitEntity && currentUnit.IsPlayerFaction)
			{
				baseUnitEntity2 = baseUnitEntity;
				goto IL_004b;
			}
		}
		baseUnitEntity2 = Game.Instance.Player.MainCharacterEntity;
		goto IL_004b;
		IL_004b:
		BaseUnitEntity baseUnitEntity3 = baseUnitEntity2;
		if (baseUnitEntity3 == null)
		{
			return null;
		}
		return entity.SelectClickInteraction(baseUnitEntity3);
	}
}
