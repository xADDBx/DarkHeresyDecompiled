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
	public readonly struct ValueDescription
	{
		public readonly StatType Type;

		public readonly int BaseValue;

		public readonly int ModifiedValue;

		public readonly IReadonlyModifiersStat Modifiers;

		public readonly StatOverride? AppliedOverride;

		public ValueDescription(ModifiableValue value)
		{
			Type = value.m_Type;
			BaseValue = value.m_ActualBaseValue;
			ModifiedValue = value.m_ModifiedValue;
			Modifiers = value.m_Modifiers;
			AppliedOverride = value.m_CurrentOverrideDescription;
		}
	}

	private static readonly Func<Modifier, bool> FilterIsPermanent = (Modifier m) => m.Permanent;

	private int m_BaseValue;

	private bool m_Forced;

	private int? m_CheatValue;

	private StatsContainer m_Container;

	private StatModifiersManager m_Modifiers;

	private StatType m_Type;

	private int m_ActualBaseValue;

	private int m_ModifiedValue;

	private int m_ModifiedValueRaw;

	private int m_PermanentValue;

	private List<ModifiableValue> m_Dependents;

	private List<EntityFact> m_DependentFacts;

	private bool m_UpdateInternalModifiers;

	private bool m_UpdateDependentFacts;

	[CanBeNull]
	private List<StatOverride> m_Overrides;

	private StatOverride? m_CurrentOverrideDescription;

	[CanBeNull]
	private ModifiableValue m_CurrentOverride;

	private int? m_OverrideModifiedValue;

	public StatType Type => m_Type;

	public int ModifiedValue => m_CurrentOverride?.ModifiedValue ?? m_ModifiedValue;

	public int PermanentValue => m_CurrentOverride?.PermanentValue ?? m_PermanentValue;

	public int BaseValue => m_CurrentOverride?.BaseValue ?? m_ActualBaseValue;

	protected StatsContainer Container => m_Container;

	public MechanicEntity Owner => m_Container?.Owner;

	public virtual int MinValue => int.MinValue;

	protected virtual bool IgnoreModifiers => false;

	public IReadonlyModifiersStat Modifiers => m_CurrentOverride?.Modifiers ?? m_Modifiers;

	public ValueDescription Description => new ValueDescription(this);

	public int? CheatValue => m_CheatValue;

	public void Initialize(StatsContainer container, StatType type)
	{
		m_Container = container;
		m_Type = type;
		m_Modifiers = new StatModifiersManager(this);
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
		m_ActualBaseValue = m_CheatValue ?? m_BaseValue;
		m_ModifiedValue = (m_ModifiedValueRaw = (m_PermanentValue = Math.Max(MinValue, m_ActualBaseValue)));
	}

	public int CalculateFilteredModifiedValue(Func<Modifier, bool> filter)
	{
		return Math.Max(MinValue, m_CheatValue ?? ApplyModifiersFiltered(m_ActualBaseValue, filter));
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
			m_ActualBaseValue = m_CheatValue ?? m_BaseValue;
			int prevValue = m_OverrideModifiedValue ?? m_ModifiedValue;
			if (m_Forced || m_CheatValue.HasValue)
			{
				m_ModifiedValue = (m_ModifiedValueRaw = (m_PermanentValue = Math.Max(MinValue, m_ActualBaseValue)));
			}
			else
			{
				m_UpdateInternalModifiers = true;
				UpdateInternalModifiers();
				m_UpdateInternalModifiers = false;
				m_ModifiedValueRaw = ApplyModifiersFiltered(m_ActualBaseValue, null);
				m_ModifiedValue = Math.Max(MinValue, m_ModifiedValueRaw);
				m_PermanentValue = Math.Max(MinValue, ApplyModifiersFiltered(m_ActualBaseValue, FilterIsPermanent));
				(StatOverride?, ModifiableValue) bestOverride = GetBestOverride();
				m_CurrentOverrideDescription = bestOverride.Item1;
				m_CurrentOverride = bestOverride.Item2;
				m_OverrideModifiedValue = m_CurrentOverride?.ModifiedValue;
			}
			HandleValueChanged(prevValue, m_OverrideModifiedValue ?? m_ModifiedValue);
		}
	}

	private (StatOverride?, ModifiableValue) GetBestOverride()
	{
		List<StatOverride> overrides = m_Overrides;
		if (overrides == null || overrides.Count <= 0)
		{
			return (null, null);
		}
		ModifiableValue modifiableValue = null;
		StatOverride? item = null;
		for (int num = m_Overrides.Count - 1; num >= 0; num--)
		{
			StatOverride value = m_Overrides[num];
			ModifiableValue stat = m_Container.GetStat(value.Type);
			if (!value.OnlyIfHigher)
			{
				modifiableValue = stat;
				item = value;
				break;
			}
			if (stat.m_ModifiedValue > m_ModifiedValue && (modifiableValue == null || stat.m_ModifiedValue > modifiableValue.m_ModifiedValue))
			{
				modifiableValue = stat;
				item = value;
			}
		}
		return (item, modifiableValue);
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
		RemoveOverride((StatOverride i) => i.SourceFact == source.Fact && i.SourceComponent == source.SourceBlueprintComponent);
	}

	public void RemoveOverride(EntityPart source)
	{
		RemoveOverride((StatOverride i) => i.SourcePart == source);
	}

	private void AddOverride(StatOverride @override)
	{
		ThrowIfOverrideIsCyclic(@override, m_Type);
		if (m_Overrides == null)
		{
			m_Overrides = new List<StatOverride>();
		}
		m_Overrides.Add(@override);
		m_Container.GetStat(@override.Type).AddDependentValue(this);
		UpdateValue();
	}

	private void RemoveOverride(Predicate<StatOverride> predicate)
	{
		List<StatOverride> overrides = m_Overrides;
		if (overrides == null || overrides.Count <= 0)
		{
			return;
		}
		m_Overrides.RemoveAll(delegate(StatOverride i)
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
		List<StatOverride> overrides = m_Container.GetStat(@override.Type).m_Overrides;
		if (overrides == null || overrides.Count <= 0)
		{
			return;
		}
		foreach (StatOverride item in overrides)
		{
			if (item.Type == statToOverride)
			{
				throw new InvalidOperationException("Cycle in stat overrides");
			}
			ThrowIfOverrideIsCyclic(item, statToOverride, depth + 1);
		}
	}
}
