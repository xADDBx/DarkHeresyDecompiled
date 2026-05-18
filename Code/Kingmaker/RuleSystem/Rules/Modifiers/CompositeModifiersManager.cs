using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class CompositeModifiersManager : AbstractModifiersManager, IReadonlyModifiersComposite, IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	private readonly int m_Min;

	private readonly int m_Max;

	public IEnumerable<Modifier> ValueModifiersList => GetList(ModifierType.ValAdd);

	public IEnumerable<Modifier> PercentModifiersList => GetList(ModifierType.PctAdd);

	public IEnumerable<Modifier> PercentMultipliersList => GetList(ModifierType.PctMul);

	public IEnumerable<Modifier> ValueModifiersExtraList => GetList(ModifierType.ValAdd_Extra);

	public IEnumerable<Modifier> PercentMultipliersExtraList => GetList(ModifierType.PctMul_Extra);

	public IEnumerable<Modifier> SortedModifiersList
	{
		get
		{
			foreach (Modifier valueModifiers in ValueModifiersList)
			{
				yield return valueModifiers;
			}
			foreach (Modifier percentModifiers in PercentModifiersList)
			{
				yield return percentModifiers;
			}
			foreach (Modifier percentMultipliers in PercentMultipliersList)
			{
				yield return percentMultipliers;
			}
			foreach (Modifier valueModifiersExtra in ValueModifiersExtraList)
			{
				yield return valueModifiersExtra;
			}
			foreach (Modifier percentMultipliersExtra in PercentMultipliersExtraList)
			{
				yield return percentMultipliersExtra;
			}
		}
	}

	public int Value => Apply(0);

	public CompositeModifiersManager(int min = int.MinValue, int max = int.MaxValue)
	{
		m_Min = min;
		m_Max = max;
	}

	public CompositeModifiersManager(int min)
		: this(min, int.MaxValue)
	{
	}

	public CompositeModifiersManager()
		: this(int.MinValue, int.MaxValue)
	{
	}

	public int Apply(int value, Func<Modifier, bool>? filter = null)
	{
		GetValues(out var valAdd, out var pctAdd, out var pctMul, out var valAddExtra, out var pctMulExtra, filter);
		return Math.Clamp(Mathf.RoundToInt(((float)(value + valAdd) * (1f + pctAdd) * pctMul + (float)valAddExtra) * pctMulExtra), m_Min, m_Max);
	}

	public int ApplyPctMulExtra(int value)
	{
		GetValues(out var _, out var _, out var _, out var _, out var pctMulExtra);
		return Math.Clamp(Mathf.RoundToInt((float)value * pctMulExtra), m_Min, m_Max);
	}

	public int GetBaseValue()
	{
		throw new NotImplementedException();
	}

	public void Add(ModifierType type, int value, EntityFact source, ModifierDescriptor descriptor)
	{
		TryAdd(new Modifier(type, value, source, null, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(ModifierType type, int value, RulebookEvent source, ModifierDescriptor descriptor)
	{
		TryAdd(new Modifier(type, value, null, null, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(ModifierType type, int value, RulebookEvent source, StatType stat)
	{
		TryAdd(new Modifier(type, value, null, null, null, BonusType.None, stat));
	}

	public void Add(ModifierType type, int value, ItemEntity source, ModifierDescriptor descriptor)
	{
		TryAdd(new Modifier(type, value, null, null, source, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(ModifierType type, int value, EntityFactComponent? source, ModifierDescriptor descriptor)
	{
		TryAdd(new Modifier(type, value, source?.Fact, source?.SourceBlueprintComponent, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(ModifierType type, int value, EntityFact? fact = null, ItemEntity? item = null, BonusType bonusType = BonusType.None, StatType stat = StatType.Unknown, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		TryAdd(new Modifier(type, value, fact, null, item, bonusType, stat, descriptor));
	}

	public void Add(Modifier modifier)
	{
		TryAdd(modifier);
	}
}
