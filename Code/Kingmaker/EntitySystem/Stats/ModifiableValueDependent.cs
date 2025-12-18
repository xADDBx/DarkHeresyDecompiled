using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Stats;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueDependent<TBaseStat> : ModifiableValue where TBaseStat : ModifiableValue
{
	private Modifier? m_BaseStatBonus;

	[CanBeNull]
	private List<StatOverride> m_BaseStatOverridesStack;

	public TBaseStat BaseStat { get; private set; }

	public virtual int BaseStatBonus => BaseStat.ModifiedValue;

	private TBaseStat GetBestBaseStat()
	{
		StatType defaultBaseStatType = base.Container.GetDefaultBaseStatType(base.Type);
		TBaseStat val = base.Container.GetStat<TBaseStat>(defaultBaseStatType);
		List<StatOverride> baseStatOverridesStack = m_BaseStatOverridesStack;
		if (baseStatOverridesStack == null || baseStatOverridesStack.Count <= 0)
		{
			return val;
		}
		foreach (StatOverride item in m_BaseStatOverridesStack)
		{
			TBaseStat stat = base.Container.GetStat<TBaseStat>(item.Type);
			if (stat.ModifiedValue > val.ModifiedValue)
			{
				val = stat;
			}
		}
		return val;
	}

	protected override void UpdateInternalModifiers()
	{
		if (BaseStat == null)
		{
			BaseStat = base.Container.GetStat<TBaseStat>(base.Container.GetDefaultBaseStatType(base.Type));
			BaseStat.AddDependentValue(this);
		}
		BaseStat = GetBestBaseStat();
		if (m_BaseStatBonus?.Value != BaseStatBonus)
		{
			RemoveModifier(m_BaseStatBonus);
			m_BaseStatBonus = AddInternalModifier(BaseStatBonus, BaseStat.Type, ModifierDescriptor.BaseStatBonus);
		}
	}

	public void AddBaseStatOverride(StatType overrideType, EntityFactComponent source, bool onlyIfHigher)
	{
		AddBaseStatOverride(new StatOverride(overrideType, source, onlyIfHigher));
	}

	public void RemoveBaseStatOverride(EntityFactComponent source)
	{
		RemoveBaseStatOverride((StatOverride i) => i.Fact == source.Fact && i.Component == source.SourceBlueprintComponent);
	}

	private void AddBaseStatOverride(StatOverride @override)
	{
		if (!((base.Container.GetStatOptional(@override.Type) ?? throw new InvalidOperationException("Can't override base stat: new base stat is missing")) is TBaseStat))
		{
			throw new InvalidOperationException("Can't override base stat: new base stat has invalid type");
		}
		if (m_BaseStatOverridesStack == null)
		{
			m_BaseStatOverridesStack = new List<StatOverride>();
		}
		m_BaseStatOverridesStack.Add(@override);
		base.Container.GetStat(@override.Type).AddDependentValue(this);
		UpdateValue();
	}

	private void RemoveBaseStatOverride(Predicate<StatOverride> predicate)
	{
		List<StatOverride> baseStatOverridesStack = m_BaseStatOverridesStack;
		if (baseStatOverridesStack == null || baseStatOverridesStack.Count <= 0)
		{
			return;
		}
		m_BaseStatOverridesStack.RemoveAll(delegate(StatOverride i)
		{
			bool num = predicate(i);
			if (num)
			{
				base.Container.GetStat(i.Type).RemoveDependentValue(this);
			}
			return num;
		});
		UpdateValue();
	}
}
public class ModifiableValueDependent : ModifiableValueDependent<ModifiableValue>
{
}
