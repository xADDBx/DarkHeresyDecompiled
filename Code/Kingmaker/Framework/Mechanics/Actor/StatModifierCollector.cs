using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Framework.Mechanics.Actor;

public sealed class StatModifierCollector
{
	private sealed class FullModifiersManager : CompositeModifiersManager
	{
		protected override bool KeepNonStackingModifiers => true;
	}

	public CompositeModifiersManager Modifiers { get; } = new FullModifiersManager();


	public List<StatOverrideEntry> FullOverrides { get; } = new List<StatOverrideEntry>();


	public List<StatOverrideEntry> BaseOverrides { get; } = new List<StatOverrideEntry>();


	public void Clear()
	{
		Modifiers.Clear();
		FullOverrides.Clear();
		BaseOverrides.Clear();
	}

	public int Apply(int baseValue)
	{
		return Modifiers.Apply(baseValue);
	}

	public void OverrideFull(StatType stat, bool onlyIfHigher, EntityFact? source)
	{
		FullOverrides.Add(new StatOverrideEntry(stat, onlyIfHigher, source));
	}

	public void OverrideBase(StatType stat, bool onlyIfHigher, EntityFact? source)
	{
		BaseOverrides.Add(new StatOverrideEntry(stat, onlyIfHigher, source));
	}

	public void AddBaseStatBonus(int value, StatType sourceStat)
	{
		Modifiers.Add(ModifierType.ValAdd, value, null, null, BonusType.None, sourceStat, ModifierDescriptor.BaseStatBonus);
	}
}
