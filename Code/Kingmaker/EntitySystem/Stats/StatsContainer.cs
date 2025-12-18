using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Stats;

public sealed class StatsContainer
{
	private static StatType?[] BaseTypes;

	private ModifiableValue?[] m_Container;

	private MechanicEntity m_Owner;

	public MechanicEntity Owner => m_Owner;

	public IEnumerable<ModifiableValue> AllStats => m_Container.Where((ModifiableValue i) => i != null);

	static StatsContainer()
	{
		BaseTypes = new StatType?[EnumUtils.GetMaxValuePlusOne<StatType>() + 1];
		RegisterBaseTypes();
	}

	public StatsContainer(MechanicEntity owner)
	{
		int num = EnumUtils.GetMaxValuePlusOne<StatType>() + 1;
		m_Container = new ModifiableValue[num];
		m_Owner = owner;
	}

	private static void RegisterBaseTypes()
	{
		foreach (var (type2, baseType2) in StatTypeHelper.BaseStats)
		{
			RegisterBaseValue(type2, baseType2);
		}
		static void RegisterBaseValue(StatType type, StatType baseType)
		{
			StatType? statType3 = BaseTypes[(int)type];
			if (statType3.HasValue)
			{
				StatType valueOrDefault = statType3.GetValueOrDefault();
				throw new Exception($"Base Type for {type} is already registered ({valueOrDefault})");
			}
			BaseTypes[(int)type] = baseType;
		}
	}

	public TModifiableValue Register<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue, new()
	{
		ModifiableValue modifiableValue = m_Container.Get((int)type);
		if (modifiableValue != null)
		{
			return (modifiableValue as TModifiableValue) ?? throw new Exception($"{Owner} modifiable value {type} already registered with different object type " + "(requested type: " + typeof(TModifiableValue).Name + ", existing type: " + modifiableValue.GetType().Name + ")");
		}
		TModifiableValue val = new TModifiableValue();
		m_Container[(int)type] = val;
		val.Initialize(this, type);
		if (Owner.IsPostLoadExecuted)
		{
			val.UpdateValue();
		}
		return val;
	}

	public ModifiableValue Register(StatType type)
	{
		return Register<ModifiableValueSimple>(type);
	}

	public ModifiableValueAttributeStat RegisterAttribute(StatType type)
	{
		return Register<ModifiableValueAttributeStat>(type);
	}

	public ModifiableValueSkill RegisterSkill(StatType type)
	{
		return Register<ModifiableValueSkill>(type);
	}

	public StatType GetDefaultBaseStatType(StatType stat)
	{
		return BaseTypes.Get((int)stat) ?? throw new Exception($"Can't find base stat for {stat}");
	}

	public void OnDidPostLoad()
	{
		foreach (ModifiableValue allStat in AllStats)
		{
			try
			{
				allStat.UpdateValue();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public ModifiableValue? GetStatOptional(StatType type)
	{
		return m_Container.Get((int)type);
	}

	public TModifiableValue? GetStatOptional<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return GetStatOptional(type) as TModifiableValue;
	}

	public ModifiableValueAttributeStat? GetAttributeOptional(StatType type)
	{
		return GetStatOptional<ModifiableValueAttributeStat>(type);
	}

	public ModifiableValueSkill? GetSkillOptional(StatType type)
	{
		return GetStatOptional<ModifiableValueSkill>(type);
	}

	public ModifiableValue GetStat(StatType type)
	{
		return GetStatOptional(type) ?? throw new Exception($"{Owner} doesn't have stat {type}");
	}

	public TModifiableValue GetStat<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return (TModifiableValue)GetStat(type);
	}

	public ModifiableValueAttributeStat GetAttribute(StatType type)
	{
		return GetStat<ModifiableValueAttributeStat>(type);
	}

	public ModifiableValueSkill GetSkill(StatType type)
	{
		return GetStat<ModifiableValueSkill>(type);
	}
}
