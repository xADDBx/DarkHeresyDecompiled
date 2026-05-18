using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class ValueModifiersManager : AbstractModifiersManager, IReadonlyModifiersValue, IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	private readonly int m_Min;

	private readonly int m_Max;

	public int Value => GetValue();

	public ValueModifiersManager(int min = int.MinValue, int max = int.MaxValue)
	{
		m_Min = min;
		m_Max = max;
	}

	public int GetValue(Func<Modifier, bool>? filter = null)
	{
		GetValues(out var valAdd, out var _, out var _, out var _, out var _, filter);
		return Math.Clamp(valAdd, m_Min, m_Max);
	}

	public void CopyFrom(IReadonlyModifiersValue other, Func<Modifier, bool>? pred = null)
	{
		CopyFrom((IReadonlyModifiers?)other, pred);
	}

	public void Add(int value, EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, value, source, null, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(int value, UnitCommand source, StatType stat)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, value, null, null, null, BonusType.None, stat));
	}

	public void Add(int value, RulebookEvent source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, value, null, null, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(int value, RulebookEvent source, StatType stat)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, value, null, null, null, BonusType.None, stat));
	}

	public void Add(int value, ItemEntity source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, value, null, null, source, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add(int value, ModifierDescriptor descriptor)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, value, null, null, null, BonusType.None, StatType.Unknown, descriptor));
	}
}
