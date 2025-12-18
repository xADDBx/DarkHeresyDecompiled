using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class FlagModifiersManager : AbstractModifiersManager, IReadonlyModifiersFlag, IReadonlyModifiers
{
	public bool Value => GetValue() > 0;

	private int GetValue([CanBeNull] Func<Modifier, bool> filter = null)
	{
		GetValues(out var valAdd, out var _, out var _, out var _, out var _, filter);
		return valAdd;
	}

	public void Add([CanBeNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, 1, source, null, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add([NotNull] EntityFactComponent runtime, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		TryAdd(new Modifier(ModifierType.ValAdd, 1, runtime.Fact, runtime.SourceBlueprintComponent, null, BonusType.None, StatType.Unknown, descriptor));
	}

	public void Add()
	{
		TryAdd(new Modifier(ModifierType.ValAdd, 1));
	}

	public static implicit operator bool(FlagModifiersManager manager)
	{
		return manager.Value;
	}
}
