using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.EntitySystem.Stats;

public abstract class ModifiableValue
{
	private int m_BaseValue;

	private bool m_Forced;

	private int? m_CheatValue;

	private StatsContainer m_Container;

	private StatsModifiersManager m_Modifiers;

	public StatType m_Type;

	public int m_ActualBaseValue;

	public int m_ModifiedValue;

	public int m_ModifiedValueRaw;

	public int m_PermanentValue;

	private List<ModifiableValue> m_Dependents;

	private List<EntityFact> m_DependentFacts;

	private bool m_UpdateInternalModifiers;

	private bool m_UpdateDependentFacts;

	[CanBeNull]
	private List<StatOverride> m_OverridesStack;

	[CanBeNull]
	private ModifiableValue m_Override;

	private int? m_OverrideModifiedValue;

	public static readonly Func<Modifier, bool> FilterIsPermanent = (Modifier m) => m.Permanent;

	public StatType Type => m_Type;

	public int ModifiedValue => m_Override?.ModifiedValue ?? m_ModifiedValue;

	public int ModifiedValueRaw => m_Override?.ModifiedValueRaw ?? m_ModifiedValueRaw;

	public int PermanentValue => m_Override?.PermanentValue ?? m_PermanentValue;

	public int BaseValue => m_Override?.BaseValue ?? m_ActualBaseValue;

	protected StatsContainer Container => m_Container;

	public MechanicEntity Owner => m_Container?.Owner;

	protected virtual int MinValue => int.MinValue;

	protected virtual bool IgnoreModifiers => false;

	public ReadonlyList<Modifier> Modifiers => m_Override?.Modifiers ?? m_Modifiers.List;

	public void Initialize(StatsContainer container, StatType type)
	{
		m_Container = container;
		m_Type = type;
		m_Modifiers = new StatsModifiersManager(this);
		CalculateBaseValue();
	}

	public void RecalculateBaseValue()
	{
		CalculateBaseValue();
		UpdateValue();
	}

	public void SetCheatValue(int? value)
	{
		m_CheatValue = value;
		UpdateValue();
	}

	private void CalculateBaseValue()
	{
		StatBaseValue statBaseValue = m_Container.Owner.GetStatBaseValue(Type);
		m_BaseValue = statBaseValue.Value;
		m_Forced = statBaseValue.Forced;
		m_ActualBaseValue = Math.Max(MinValue, m_CheatValue ?? m_BaseValue);
		m_ModifiedValue = (m_ModifiedValueRaw = (m_PermanentValue = m_ActualBaseValue));
	}

	public int CalculateFilteredModifiedValue(Func<Modifier, bool> filter)
	{
		return Math.Max(MinValue, m_CheatValue ?? ApplyModifiersFiltered(m_ActualBaseValue, filter));
	}

	protected int CalculatePermanentValue()
	{
		return ApplyModifiersFiltered(m_ActualBaseValue, FilterIsPermanent);
	}

	public void AddDependentValue(ModifiableValue dependent)
	{
		if (m_Dependents == null)
		{
			m_Dependents = new List<ModifiableValue>();
		}
		if (!m_Dependents.HasItem(dependent))
		{
			m_Dependents.Add(dependent);
		}
	}

	public void RemoveDependentValue(ModifiableValue dependent)
	{
		if (m_Dependents == null || !m_Dependents.Remove(dependent))
		{
			PFLog.Default.Error("Error in RemoveDependentValue");
		}
	}

	private bool AddModifier(Modifier mod)
	{
		bool result = m_Modifiers.Add(mod);
		if (!IgnoreModifiers)
		{
			UpdateValue();
		}
		return result;
	}

	public IEnumerable<Modifier> GetDisplayModifiers()
	{
		return (m_Override?.m_Modifiers ?? m_Modifiers).GetDisplayModifiers();
	}

	public bool RemoveModifier(Modifier? mod)
	{
		if (!mod.HasValue)
		{
			return false;
		}
		bool num = m_Modifiers.RemoveAll((Modifier i) => i.Equals(mod));
		if (num)
		{
			UpdateValue();
		}
		return num;
	}

	public void RemoveModifiersFrom(EntityFactComponent source)
	{
		if (m_Modifiers.RemoveAll((Modifier i) => i.Fact == source.Fact && i.Component == source.SourceBlueprintComponent))
		{
			UpdateValue();
		}
	}

	public IEnumerable<Modifier> GetModifiers(ModifierDescriptor descriptor)
	{
		return Modifiers.Where((Modifier i) => i.Descriptor == descriptor);
	}

	public Modifier? AddModifier(ModifierType type, int value, [NotNull] EntityFactComponent source, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		Modifier modifier = new Modifier(type, value, source.Fact, source.SourceBlueprintComponent, null, BonusType.None, StatType.Unknown, desc);
		if (!AddModifier(modifier))
		{
			return null;
		}
		return modifier;
	}

	public Modifier? AddItemModifier(ModifierType type, int value, [NotNull] ItemEntity itemSource, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		Modifier modifier = new Modifier(type, value, null, null, itemSource, BonusType.None, StatType.Unknown, desc);
		if (!AddModifier(modifier))
		{
			return null;
		}
		return modifier;
	}

	protected Modifier? AddInternalModifier(ModifierType type, int value, StatType sourceStat = StatType.Unknown, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		Modifier modifier = new Modifier(type, value, null, null, null, BonusType.None, sourceStat, desc);
		if (!AddModifier(modifier))
		{
			return null;
		}
		return modifier;
	}

	protected Modifier? AddInternalModifier(int value, StatType sourceStat = StatType.Unknown, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		return AddInternalModifier(ModifierType.ValAdd, value, sourceStat, desc);
	}

	public Modifier? AddModifier(int value, [NotNull] EntityFactComponent source, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		return AddModifier(ModifierType.ValAdd, value, source, desc);
	}

	public Modifier? AddItemModifier(int value, [NotNull] ItemEntity itemSource, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		return AddItemModifier(ModifierType.ValAdd, value, itemSource, desc);
	}

	public void UpdateValue()
	{
		if (!m_UpdateInternalModifiers && !m_UpdateDependentFacts)
		{
			m_ActualBaseValue = Math.Max(MinValue, m_CheatValue ?? m_BaseValue);
			int prevValue = m_OverrideModifiedValue ?? m_ModifiedValue;
			if (m_Forced || m_CheatValue.HasValue)
			{
				m_ModifiedValue = (m_ModifiedValueRaw = (m_PermanentValue = m_ActualBaseValue));
			}
			else
			{
				m_UpdateInternalModifiers = true;
				UpdateInternalModifiers();
				m_UpdateInternalModifiers = false;
				m_ModifiedValueRaw = ApplyModifiersFiltered(m_ActualBaseValue, null);
				m_ModifiedValue = Math.Max(MinValue, m_ModifiedValueRaw);
				m_PermanentValue = CalculatePermanentValue();
				m_Override = GetBestOverride();
				m_OverrideModifiedValue = m_Override?.ModifiedValue;
			}
			HandleValueChanged(prevValue, m_OverrideModifiedValue ?? m_ModifiedValue);
		}
	}

	[CanBeNull]
	private ModifiableValue GetBestOverride()
	{
		List<StatOverride> overridesStack = m_OverridesStack;
		if (overridesStack == null || overridesStack.Count <= 0)
		{
			return null;
		}
		ModifiableValue modifiableValue = null;
		for (int num = m_OverridesStack.Count - 1; num >= 0; num--)
		{
			StatOverride statOverride = m_OverridesStack[num];
			ModifiableValue stat = m_Container.GetStat(statOverride.Type);
			if (!statOverride.OnlyIfHigher)
			{
				modifiableValue = stat;
				break;
			}
			if (stat.m_ModifiedValue > m_ModifiedValue && (modifiableValue == null || stat.m_ModifiedValue > modifiableValue.m_ModifiedValue))
			{
				modifiableValue = stat;
			}
		}
		return modifiableValue;
	}

	private void HandleValueChanged(int prevValue, int currentValue)
	{
		if (prevValue == currentValue)
		{
			return;
		}
		UpdateDependentFactsAndComponents();
		EventBus.RaiseEvent(delegate(IModifiableValueChangedHandler h)
		{
			h.HandleModifiableValueChanged(this);
		});
		if (m_Dependents == null)
		{
			return;
		}
		List<ModifiableValue> list;
		using (m_Dependents.ToPooledList(out list))
		{
			foreach (ModifiableValue item in list)
			{
				item.UpdateValue();
			}
		}
	}

	public void AddDependentFact(EntityFact fact)
	{
		if (m_DependentFacts == null)
		{
			m_DependentFacts = new List<EntityFact>();
		}
		if (!m_DependentFacts.HasItem(fact))
		{
			m_DependentFacts.Add(fact);
		}
	}

	public void RemoveDependentFact(EntityFact fact)
	{
		m_DependentFacts?.Remove(fact);
	}

	private void UpdateDependentFactsAndComponents()
	{
		MechanicEntity owner = Owner;
		if ((owner != null && owner.IsDisposingNow) || m_UpdateDependentFacts)
		{
			return;
		}
		m_UpdateDependentFacts = true;
		if (m_DependentFacts != null)
		{
			foreach (EntityFact item in m_DependentFacts.ToList())
			{
				try
				{
					item.Reapply();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
		m_UpdateDependentFacts = false;
	}

	protected virtual void UpdateInternalModifiers()
	{
	}

	private int ApplyModifiersFiltered(int baseValue, Func<Modifier, bool> filter)
	{
		if (!IgnoreModifiers)
		{
			return m_Modifiers.Apply(baseValue, filter);
		}
		return baseValue;
	}

	public static implicit operator int([CanBeNull] ModifiableValue v)
	{
		return v?.ModifiedValue ?? 0;
	}

	public void AddOverride(StatType overrideType, EntityFactComponent source, bool onlyIfHigher)
	{
		AddOverride(new StatOverride(overrideType, source, onlyIfHigher));
	}

	public void AddOverride(StatType overrideType, EntityPart source, bool onlyIfHigher)
	{
		AddOverride(new StatOverride(overrideType, source, onlyIfHigher));
	}

	public void RemoveOverride(EntityFactComponent source)
	{
		RemoveOverride((StatOverride i) => i.Fact == source.Fact && i.Component == source.SourceBlueprintComponent);
	}

	public void RemoveOverride(EntityPart source)
	{
		RemoveOverride((StatOverride i) => i.Part == source);
	}

	private void AddOverride(StatOverride @override)
	{
		ThrowIfOverrideIsCyclic(@override, m_Type);
		if (m_OverridesStack == null)
		{
			m_OverridesStack = new List<StatOverride>();
		}
		m_OverridesStack.Add(@override);
		m_Container.GetStat(@override.Type).AddDependentValue(this);
		UpdateValue();
	}

	private void RemoveOverride(Predicate<StatOverride> predicate)
	{
		List<StatOverride> overridesStack = m_OverridesStack;
		if (overridesStack == null || overridesStack.Count <= 0)
		{
			return;
		}
		m_OverridesStack.RemoveAll(delegate(StatOverride i)
		{
			bool num = predicate(i);
			if (num)
			{
				m_Container.GetStat(i.Type).RemoveDependentValue(this);
			}
			return num;
		});
		UpdateValue();
	}

	private void ThrowIfOverrideIsCyclic(StatOverride @override, StatType statToOverride, int depth = 0)
	{
		if (depth > 100)
		{
			throw new StackOverflowException("Cycle in stat overrides");
		}
		List<StatOverride> overridesStack = m_Container.GetStat(@override.Type).m_OverridesStack;
		if (overridesStack == null || overridesStack.Count <= 0)
		{
			return;
		}
		foreach (StatOverride item in overridesStack)
		{
			if (item.Type == statToOverride)
			{
				throw new InvalidOperationException("Cycle in stat overrides");
			}
			ThrowIfOverrideIsCyclic(item, statToOverride, depth + 1);
		}
	}
}
