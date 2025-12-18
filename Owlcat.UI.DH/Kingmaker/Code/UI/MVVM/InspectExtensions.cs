using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Enums;
using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InspectExtensions
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
		return Rulebook.Trigger(new RuleCalculateDefence(unit, unit)).ResultDefence;
	}

	public static string GetDefence(BaseUnitEntity unit)
	{
		int defenceValue = GetDefenceValue(unit);
		return $"{defenceValue}%";
	}

	public static string GetDamageReduction(BaseUnitEntity unit)
	{
		ModifiableValue statOptional = unit.GetStatOptional(StatType.ArmorDamageReduction);
		if (statOptional != null)
		{
			return $"{(int)statOptional}%";
		}
		return "0%";
	}

	public static string GetMovementPoints(BaseUnitEntity unit)
	{
		return unit.CombatState.MovementPointsMax.ToString();
	}

	public static List<TooltipBrickBuff> GetBuffs(BaseUnitEntity unit)
	{
		return (from b in unit.Buffs.RawFacts.ToList()
			where !b.Blueprint.IsHiddenInUI
			select b).Select(delegate(Buff buff)
		{
			BuffGroupType group = (buff.Blueprint.IsDOTVisual ? BuffGroupType.DOT : ((!buff.Blueprint.IsCriticalEffect) ? (unit.IsEnemy(buff.Context.MaybeCaster) ? BuffGroupType.Negative : BuffGroupType.Positive) : BuffGroupType.CriticalEffect));
			return new TooltipBrickBuff(buff, group);
		}).ToList();
	}

	public static ObservableList<ITooltipBrick> GetBuffsTooltipBricks(MechanicEntity unit)
	{
		return new ObservableList<ITooltipBrick>((from b in unit.Buffs.RawFacts.ToList()
			where !b.Blueprint.IsHiddenInUI
			select b).Select((Func<Buff, ITooltipBrick>)((Buff buff) => new TooltipBrickBuff(buff, GetBuffGroupType(buff, unit)))));
	}

	private static BuffGroupType GetBuffGroupType(Buff buff, MechanicEntity unit)
	{
		BuffUISettings buffUISettings = buff.Blueprint.BuffUISettings;
		if (buffUISettings != null)
		{
			BuffGroupType group = buffUISettings.GetGroup(GetBuffTargetType(unit));
			if (group != BuffGroupType.None)
			{
				return group;
			}
		}
		if (buff.Blueprint.IsDOTVisual)
		{
			return BuffGroupType.DOT;
		}
		if (buff.Blueprint.IsCriticalEffect)
		{
			return BuffGroupType.CriticalEffect;
		}
		return unit.IsEnemy(buff.Context.MaybeCaster) ? BuffGroupType.Negative : BuffGroupType.Positive;
	}

	private static BuffTargetType GetBuffTargetType(MechanicEntity unit)
	{
		if (unit.IsPlayerEnemy)
		{
			return BuffTargetType.Enemy;
		}
		if (unit.IsPlayerFaction)
		{
			return BuffTargetType.Ally;
		}
		return BuffTargetType.All;
	}
}
