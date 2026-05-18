using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using ObservableCollections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class InspectExtensions
{
	public static bool TryGetWoundsText(MechanicEntityUIWrapper unitUIWrapper, out string woundsValue)
	{
		if (unitUIWrapper.Health == null)
		{
			woundsValue = string.Empty;
			return false;
		}
		if (unitUIWrapper.MechanicEntity.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			woundsValue = "???";
			return true;
		}
		woundsValue = UIUtilityUnit.GetHpText(unitUIWrapper, unitUIWrapper.IsDead);
		return true;
	}

	public static bool TryGetDurabilityText(MechanicEntityUIWrapper unitUIWrapper, out string durabilityValue)
	{
		if (unitUIWrapper.Armor == null)
		{
			durabilityValue = string.Empty;
			return false;
		}
		if (unitUIWrapper.MechanicEntity.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			durabilityValue = "???";
			return true;
		}
		durabilityValue = UIUtilityUnit.GetArmorText(unitUIWrapper, unitUIWrapper.IsDead);
		return true;
	}

	public static int GetDefenceValue(BaseUnitEntity unit)
	{
		return unit.GetEffectiveDefence();
	}

	public static string GetDefence(BaseUnitEntity unit)
	{
		int defenceValue = GetDefenceValue(unit);
		return $"{defenceValue}%";
	}

	public static string GetDamageReduction(BaseUnitEntity unit)
	{
		return string.Format("{0}%", unit.Actor.GetStat(StatType.ArmorDamageReduction, null, default(StatContext), "GetDamageReduction").ModifiedValue);
	}

	public static string GetMovementPoints(BaseUnitEntity unit)
	{
		return Mathf.Max(unit.CombatState.MovementPointsMax, 0).ToString();
	}

	public static IUIUnitMoraleData GetMorale(BaseUnitEntity unit)
	{
		IUIUnitMoraleData iUIUnitMoraleData = unit?.Parts?.GetOptional<PartMorale>();
		if (iUIUnitMoraleData == null)
		{
			return null;
		}
		return iUIUnitMoraleData;
	}

	public static List<BrickBuffVM> GetBuffs(BaseUnitEntity unit)
	{
		return (from b in unit.Buffs.RawFacts.ToList()
			where !b.Blueprint.IsHiddenInUI
			select b into buff
			select new BrickBuffVM(buff)).ToList();
	}

	public static ObservableList<ITooltipBrick> GetBuffsTooltipBricks(MechanicEntity unit)
	{
		return new ObservableList<ITooltipBrick>(from b in unit.Buffs.RawFacts.ToList()
			where !b.Blueprint.IsHiddenInUI
			select b into buff
			select new BrickBuffVM(buff));
	}

	public static BuffGroupFlags GetGroupsShowFlags(this UnitUIInspectSettings settings)
	{
		BuffGroupFlags buffGroupFlags = BuffGroupFlags.All;
		if (settings == null)
		{
			return buffGroupFlags;
		}
		if (settings.HasFlags(UnitInspectUIFlags.HideCriticalEffects))
		{
			buffGroupFlags &= ~BuffGroupFlags.CriticalEffects;
		}
		if (settings.HasFlags(UnitInspectUIFlags.HideStatusEffects))
		{
			buffGroupFlags &= ~BuffGroupFlags.StatusEffects;
		}
		if (settings.HasFlags(UnitInspectUIFlags.HideDotEffects))
		{
			buffGroupFlags &= ~BuffGroupFlags.DotEffects;
		}
		if (settings.HasFlags(UnitInspectUIFlags.HideNegativeEffects))
		{
			buffGroupFlags &= ~BuffGroupFlags.NegativeEffects;
		}
		if (settings.HasFlags(UnitInspectUIFlags.HidePositiveEffects))
		{
			buffGroupFlags &= ~BuffGroupFlags.PositiveEffects;
		}
		return buffGroupFlags;
	}
}
