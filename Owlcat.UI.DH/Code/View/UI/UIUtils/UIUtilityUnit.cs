using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.View.Mechadendrites;
using UnityEngine;

namespace Code.View.UI.UIUtils;

public static class UIUtilityUnit
{
	private static EnumUnitSubtypeIcons SubtypeIcons => UIConfig.Instance.Portraits.UnitSubtypeIcons;

	private static EnumUnitSubtypeIcons SubtypePortrait => UIConfig.Instance.Portraits.UnitSubtypePortrait;

	public static IEnumerable<FeatureSelectorSlotVM> CollectAbilitiesVMs(BaseUnitEntity unit)
	{
		return from a in UtilityUnit.CollectAbilities(unit)
			select new FeatureSelectorSlotVM(a, unit);
	}

	public static IEnumerable<ToggleAbility> CollectToggleAbilities(BaseUnitEntity unit)
	{
		return unit.ToggleAbilities.Visible;
	}

	public static IEnumerable<Feature> CollectFeatures(BaseUnitEntity unit)
	{
		return from f in unit.Progression.Features.Visible
			where !f.Blueprint.HideInCharacterSheetAndLevelUp
			where f.Blueprint.Name != string.Empty
			select f;
	}

	public static IEnumerable<UIFeature> CollectFeats(BaseUnitEntity unit)
	{
		return from f in unit.Progression.Features.Visible
			where !f.Blueprint.HideInCharacterSheetAndLevelUp
			where f.Blueprint.Name != string.Empty
			select new UIFeature(f, f.Param);
	}

	public static string GetHpText(MechanicEntityUIWrapper unit, bool isDead, int hpLeftSize = 80)
	{
		int num = ((!isDead) ? unit.Health.MaxHitPoints : (((int?)unit.MechanicEntity?.Actor.GetStat(StatType.Toughness, null, default(StatContext), "GetHpText")) ?? 0));
		int temporaryHitPoints = unit.Health.TemporaryHitPoints;
		int hitPointsLeft = unit.Health.HitPointsLeft;
		string arg = $"{num}";
		string arg2 = $"{hitPointsLeft + temporaryHitPoints}";
		return $"{arg2}/<size={hpLeftSize}%>{arg}";
	}

	public static string GetArmorText(MechanicEntityUIWrapper unit, bool isDead, int armorLeftSize = 80)
	{
		PartArmor armor = unit.Armor;
		if (armor == null)
		{
			return "0/0";
		}
		return $"{armor.DurabilityLeft}/<size={armorLeftSize}%>{armor.DurabilityValue}";
	}

	public static bool MoraleHasPrediction(IUIUnitMoraleData currentMorale, IUIUnitMoraleData predictedMorale)
	{
		return predictedMorale.Morale != currentMorale.Morale;
	}

	public static bool MoraleWillBecomeHeroic(IUIUnitMoraleData currentMorale, IUIUnitMoraleData predictedMorale)
	{
		if (predictedMorale == null || currentMorale == null)
		{
			return false;
		}
		if (predictedMorale.MoralePhase == MoralePhaseType.Heroic)
		{
			return currentMorale.MoralePhase != MoralePhaseType.Heroic;
		}
		return false;
	}

	public static bool MoraleWillBecomeBroken(IUIUnitMoraleData currentMorale, IUIUnitMoraleData predictedMorale)
	{
		if (predictedMorale == null || currentMorale == null)
		{
			return false;
		}
		if (predictedMorale.MoralePhase == MoralePhaseType.Broken)
		{
			return currentMorale.MoralePhase != MoralePhaseType.Broken;
		}
		return false;
	}

	public static string SoulMarkShiftsText(List<AlignmentShift> shifts, SoulMarkShiftColors colors)
	{
		string text = "";
		if (shifts == null)
		{
			return text;
		}
		foreach (AlignmentShift shift in shifts)
		{
			if (!shift.NoShift)
			{
				AlignmentAxis axis = shift.Axis;
				int value = shift.Value;
				string arg = ColorUtility.ToHtmlStringRGB((Color)((shift.Value > 0) ? colors.SoulMarkShiftBePositive : colors.SoulMarkShiftBeNegative));
				text += string.Format(UIStrings.Instance.Dialog.SoulMarkShiftFormat.Text, UIUtilityAlignment.GetAlignmentDirectionText(axis).Text, value, arg);
				text += "\n\n";
			}
		}
		return text;
	}

	public static int GetWeaponSetsCount(BaseUnitEntity unit)
	{
		if (!unit.HasMechadendrites())
		{
			return 2;
		}
		return 1;
	}

	public static IEnumerable<Ability> CollectAbilities(BaseUnitEntity unit)
	{
		return UtilityUnit.CollectAbilities(unit);
	}

	public static IEnumerable<TBlueprint> GetBlueprintUnitFactFromFact<TBlueprint>(BlueprintMechanicEntityFact blueprintMechanicEntityFact) where TBlueprint : BlueprintUnitFact
	{
		return UtilityUnit.GetBlueprintUnitFactFromFact<TBlueprint>(blueprintMechanicEntityFact);
	}

	public static StatType? GetSourceStatType(StatType statType)
	{
		return UtilityUnit.GetSourceStatType(statType);
	}

	public static int GetSurfaceEnemyDifficulty([CanBeNull] BaseUnitEntity unit)
	{
		return UtilityUnit.GetSurfaceEnemyDifficulty(unit);
	}

	public static bool UsedSubtypeIcon(MechanicEntity mechanicEntityEntity)
	{
		return UtilityUnit.UsedSubtypeIcon(mechanicEntityEntity);
	}

	public static Sprite GetSurfaceCombatStandardPortrait(MechanicEntity mechanicEntityEntity, PortraitCombatSize size)
	{
		return UtilityUnit.GetSurfaceCombatStandardPortrait(mechanicEntityEntity, size);
	}

	public static bool IsCastingAbility([CanBeNull] this BaseUnitEntity unit)
	{
		return UtilityUnit.IsCastingAbility(unit);
	}

	public static bool InPartyAndControllable(this MechanicEntity unit)
	{
		return UtilityUnit.InPartyAndControllable(unit);
	}

	public static StatType? TryGetStatType(string key)
	{
		return UtilityUnit.TryGetStatType(key);
	}

	public static bool IsViewActiveUnit(BaseUnitEntity unit)
	{
		return UtilityUnit.IsViewActiveUnit(unit);
	}
}
