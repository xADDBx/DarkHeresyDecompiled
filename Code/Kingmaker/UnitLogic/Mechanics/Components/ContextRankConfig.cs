using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintAreaEffect))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("a15673bd9e3879442825226335fd8fe2")]
public class ContextRankConfig : BlueprintComponent
{
	[Serializable]
	private class CustomProgressionItem
	{
		[UsedImplicitly]
		public int BaseValue;

		[UsedImplicitly]
		public int ProgressionValue;
	}

	[SerializeField]
	[Tooltip("Symbolic link to corresponding field in <DealDamage> components. Has no intrinsic meaning")]
	private AbilityRankType m_Type;

	[SerializeField]
	private ContextRankBaseValueType m_BaseValueType;

	[SerializeField]
	[ShowIf("IsBasedOnFeatureRank")]
	private BlueprintFeatureReference m_Feature;

	[SerializeField]
	[ShowIf("IsFeatureList")]
	private BlueprintFeatureReference[] m_FeatureList;

	[SerializeField]
	[ShowIf("IsBasedOnStatBonus")]
	private StatType m_Stat;

	[SerializeField]
	[ShowIf("IsBasedOnBuffRank")]
	private BlueprintBuffReference m_Buff;

	[SerializeField]
	private ContextRankProgression m_Progression;

	[SerializeField]
	[ShowIf("IsCustomProgression")]
	private CustomProgressionItem[] m_CustomProgression = new CustomProgressionItem[0];

	[SerializeField]
	[ShowIf("IsDivisionProgressionStart")]
	private int m_StartLevel;

	[SerializeField]
	[ShowIf("IsDivisionProgression")]
	private int m_StepLevel;

	[SerializeField]
	[Tooltip("Allows to set minimum value. When calculated result is less then set minimum, minumum value will be used")]
	private bool m_UseMin;

	[SerializeField]
	[ShowIf("m_UseMin")]
	private int m_Min;

	[SerializeField]
	[Tooltip("Allows to set maximum value. When calculated result is greater then set maximum, maximum value will be used")]
	private bool m_UseMax;

	[SerializeField]
	[ShowIf("m_UseMax")]
	private int m_Max = 20;

	[SerializeField]
	[ShowIf("IsBasedOnCustomProperty")]
	private BlueprintEntityPropertyReference m_CustomProperty;

	[SerializeField]
	[ShowIf("IsBasedOnCustomPropertyList")]
	private BlueprintEntityPropertyReference[] m_CustomPropertyList;

	public bool HasFeature
	{
		get
		{
			if (m_Feature != null)
			{
				return !m_Feature.IsEmpty();
			}
			return false;
		}
	}

	public bool HasProperty
	{
		get
		{
			if (m_CustomProperty != null)
			{
				return !m_CustomProperty.IsEmpty();
			}
			return false;
		}
	}

	public bool IsBasedOnFeatureRank
	{
		get
		{
			if (m_BaseValueType != ContextRankBaseValueType.FeatureRank)
			{
				return m_BaseValueType == ContextRankBaseValueType.MasterFeatureRank;
			}
			return true;
		}
	}

	public bool IsBasedOnStatBonus
	{
		get
		{
			if (m_BaseValueType != ContextRankBaseValueType.StatBonus && m_BaseValueType != ContextRankBaseValueType.BaseStat && m_BaseValueType != ContextRankBaseValueType.WarhammerStatBonus && m_BaseValueType != ContextRankBaseValueType.WarhammerStatBonusPlusFeatureList)
			{
				return m_BaseValueType == ContextRankBaseValueType.Stat;
			}
			return true;
		}
	}

	public bool IsFeatureList
	{
		get
		{
			if (m_BaseValueType != ContextRankBaseValueType.FeatureList && m_BaseValueType != ContextRankBaseValueType.FeatureListRanks)
			{
				return m_BaseValueType == ContextRankBaseValueType.WarhammerStatBonusPlusFeatureList;
			}
			return true;
		}
	}

	public bool IsBasedOnCustomProperty => m_BaseValueType == ContextRankBaseValueType.CustomProperty;

	public bool IsBasedOnCustomPropertyList => m_BaseValueType == ContextRankBaseValueType.MaxCustomProperty;

	public bool IsDivisionProgression
	{
		get
		{
			if (m_Progression != ContextRankProgression.StartPlusDivStep && m_Progression != ContextRankProgression.DivStep && m_Progression != ContextRankProgression.OnePlusDivStep && m_Progression != ContextRankProgression.MultiplyByModifier && m_Progression != ContextRankProgression.BonusValue && m_Progression != ContextRankProgression.MultiplyByStepPlusStart && m_Progression != ContextRankProgression.DivByStepPlusStart && m_Progression != ContextRankProgression.DoublePlusBonusValue)
			{
				return m_Progression == ContextRankProgression.Div2PlusStep;
			}
			return true;
		}
	}

	public bool IsDivisionProgressionStart
	{
		get
		{
			if (m_Progression != ContextRankProgression.StartPlusDivStep && m_Progression != ContextRankProgression.MultiplyByStepPlusStart)
			{
				return m_Progression == ContextRankProgression.DivByStepPlusStart;
			}
			return true;
		}
	}

	private bool IsCustomProgression => m_Progression == ContextRankProgression.Custom;

	public bool IsBasedOnBuffRank
	{
		get
		{
			if (m_BaseValueType != ContextRankBaseValueType.CasterBuffRank)
			{
				return m_BaseValueType == ContextRankBaseValueType.TargetBuffRank;
			}
			return true;
		}
	}

	public AbilityRankType Type => m_Type;

	public int GetValue(MechanicsContext context)
	{
		BlueprintAbility ability = base.OwnerBlueprint as BlueprintAbility;
		MechanicEntity maybeCaster = context.MaybeCaster;
		bool unlimited = maybeCaster != null && maybeCaster.GetOptional<UnitPartUnlimitedSpells>()?.CheckUnlimitedEntry(ability) == true;
		return ApplyMinMax(ApplyProgression(GetBaseValue(context)), unlimited);
	}

	private int GetBaseValue(MechanicsContext context)
	{
		BaseUnitEntity caster = context.MaybeCaster as BaseUnitEntity;
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return 0;
		}
		switch (m_BaseValueType)
		{
		case ContextRankBaseValueType.CasterLevel:
			return 0;
		case ContextRankBaseValueType.FeatureRank:
			return caster.Progression.Features.GetRank(m_Feature.Get());
		case ContextRankBaseValueType.StatBonus:
			return caster.GetStatOptional<ModifiableValueAttributeStat>(m_Stat)?.Bonus ?? 0;
		case ContextRankBaseValueType.CharacterLevel:
			return caster.Progression.CharacterLevel;
		case ContextRankBaseValueType.FeatureList:
		{
			int num5 = 0;
			BlueprintFeatureReference[] featureList = m_FeatureList;
			foreach (BlueprintFeatureReference blueprintFeatureReference2 in featureList)
			{
				if (caster.Progression.Features.Contains(blueprintFeatureReference2.Get()))
				{
					num5++;
				}
			}
			return num5;
		}
		case ContextRankBaseValueType.FeatureListRanks:
		{
			int num6 = 0;
			BlueprintFeatureReference[] featureList = m_FeatureList;
			foreach (BlueprintFeatureReference blueprintFeatureReference3 in featureList)
			{
				num6 += caster.Progression.Features.GetRank(blueprintFeatureReference3.Get());
			}
			return num6;
		}
		case ContextRankBaseValueType.MasterFeatureRank:
			return caster.Master?.Progression.Features.GetRank(m_Feature.Get()) ?? 0;
		case ContextRankBaseValueType.CasterCR:
			return 0;
		case ContextRankBaseValueType.BaseStat:
			return caster.Stats.GetStatOptional(m_Stat)?.BaseValue ?? 0;
		case ContextRankBaseValueType.CustomProperty:
			return SimpleBlueprintExtendAsObject.Or(m_CustomProperty.Get(), null)?.GetValue(new PropertyContext(caster)) ?? 0;
		case ContextRankBaseValueType.CasterBuffRank:
			return (caster?.Facts.Get(m_Buff.Get())?.GetRank()).GetValueOrDefault();
		case ContextRankBaseValueType.TargetBuffRank:
			return (context.ClickedTarget.Entity?.Facts.Get(m_Buff.Get())?.GetRank()).GetValueOrDefault();
		case ContextRankBaseValueType.MaxCustomProperty:
		{
			int num3 = 0;
			BlueprintEntityPropertyReference[] customPropertyList = m_CustomPropertyList;
			for (int i = 0; i < customPropertyList.Length; i++)
			{
				int num4 = SimpleBlueprintExtendAsObject.Or(customPropertyList[i].Get(), null)?.GetValue(new PropertyContext(caster)) ?? 0;
				if (num4 > num3)
				{
					num3 = num4;
				}
			}
			return num3;
		}
		case ContextRankBaseValueType.WarhammerStatBonus:
			return caster.Stats.GetStatOptional<ModifiableValueAttributeStat>(m_Stat)?.WarhammerBonus ?? 0;
		case ContextRankBaseValueType.CurrentWeaponRateOfFire:
			if (caster == null || context.SourceAbility?.Weapon == null)
			{
				return Rulebook.Trigger(new RuleCalculateBurstCount(caster)).Result;
			}
			return Rulebook.Trigger(new RuleCalculateBurstCount(caster, context.SourceAbility)).Result;
		case ContextRankBaseValueType.EnemiesAdjacent:
			if (caster == null)
			{
				return 0;
			}
			return Game.Instance.EntityPools.AllUnits.Count((AbstractUnitEntity p) => p.DistanceToInCells(caster) <= 1);
		case ContextRankBaseValueType.Stat:
			return caster.Stats.GetStatOptional(m_Stat)?.ModifiedValue ?? 0;
		case ContextRankBaseValueType.WarhammerStatBonusPlusFeatureList:
		{
			int num = caster.Stats.GetStatOptional<ModifiableValueAttributeStat>(m_Stat)?.WarhammerBonus ?? 0;
			int num2 = 0;
			BlueprintFeatureReference[] featureList = m_FeatureList;
			foreach (BlueprintFeatureReference blueprintFeatureReference in featureList)
			{
				num2 += caster.Progression.Features.GetRank(blueprintFeatureReference.Get());
			}
			return num + num2;
		}
		default:
			PFLog.Default.Error(context.Blueprint, $"Invalid rank base value: {context.Blueprint}");
			return 0;
		}
	}

	private int ApplyProgression(int value)
	{
		switch (m_Progression)
		{
		case ContextRankProgression.AsIs:
			return value;
		case ContextRankProgression.Div2:
			return value / 2;
		case ContextRankProgression.Divide2RoundUp:
			return (value + 1) / 2;
		case ContextRankProgression.StartPlusDivStep:
			return value / m_StepLevel + m_StartLevel;
		case ContextRankProgression.DivStep:
			return value / m_StepLevel;
		case ContextRankProgression.OnePlusDivStep:
			return 1 + value / m_StepLevel;
		case ContextRankProgression.MultiplyByModifier:
			return value * m_StepLevel;
		case ContextRankProgression.HalfMore:
			return value + value / 2;
		case ContextRankProgression.BonusValue:
			return value + m_StepLevel;
		case ContextRankProgression.MultiplyByStepPlusStart:
			return value * m_StepLevel + m_StartLevel;
		case ContextRankProgression.DivByStepPlusStart:
			return value / m_StepLevel + m_StartLevel;
		case ContextRankProgression.Div2PlusStep:
			return value / 2 + m_StepLevel;
		case ContextRankProgression.Custom:
		{
			CustomProgressionItem[] customProgression = m_CustomProgression;
			foreach (CustomProgressionItem customProgressionItem in customProgression)
			{
				if (value <= customProgressionItem.BaseValue)
				{
					return customProgressionItem.ProgressionValue;
				}
			}
			return m_CustomProgression.LastItem()?.ProgressionValue ?? 0;
		}
		case ContextRankProgression.DoublePlusBonusValue:
			return value * 2 + m_StepLevel;
		default:
			throw new ArgumentOutOfRangeException("m_Progression", m_Progression, null);
		}
	}

	private int ApplyMinMax(int value, bool unlimited)
	{
		if (m_UseMin)
		{
			value = Math.Max(value, m_Min);
		}
		if (m_UseMax && !unlimited)
		{
			value = Math.Min(value, m_Max);
		}
		return value;
	}
}
