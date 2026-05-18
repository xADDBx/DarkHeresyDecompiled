using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public sealed class GlobalStatModifierRegistry
{
	public readonly struct RegisteredGlobalModifier
	{
		public readonly EntityFactComponent Component;

		public readonly IStatModifier Modifier;

		public readonly bool IsConditional;

		public readonly StatType[]? DependsOnStats;

		public RegisteredGlobalModifier(EntityFactComponent component, IStatModifier modifier, bool isConditional, StatType[]? dependsOnStats)
		{
			Component = component;
			Modifier = modifier;
			IsConditional = isConditional;
			DependsOnStats = dependsOnStats;
		}
	}

	private static GlobalStatModifierRegistry? _Instance;

	private readonly List<RegisteredGlobalModifier>?[] _modifiersByStat;

	public static GlobalStatModifierRegistry Instance => _Instance ?? (_Instance = new GlobalStatModifierRegistry());

	private GlobalStatModifierRegistry()
	{
		_modifiersByStat = new List<RegisteredGlobalModifier>[StatTypeHelper.AllStatsArraySize];
	}

	public void Register(EntityFactComponent component, IStatModifier modifier, StatType stat, bool isConditional, StatType[]? dependsOnStats)
	{
		List<RegisteredGlobalModifier>[] modifiersByStat = _modifiersByStat;
		(modifiersByStat[(int)stat] ?? (modifiersByStat[(int)stat] = new List<RegisteredGlobalModifier>())).Add(new RegisteredGlobalModifier(component, modifier, isConditional, dependsOnStats));
	}

	public void Unregister(EntityFactComponent component, StatType stat)
	{
		List<RegisteredGlobalModifier> list = _modifiersByStat[(int)stat];
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Component == component)
			{
				list.RemoveAt(num);
				break;
			}
		}
	}

	public void IterateModifiers(StatModifierCollector collector, StatType stat, StatContext ctx)
	{
		List<RegisteredGlobalModifier> list = _modifiersByStat[(int)stat];
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			RegisteredGlobalModifier registeredGlobalModifier = list[i];
			using (registeredGlobalModifier.Component.SetScope())
			{
				registeredGlobalModifier.Modifier.TryApplyStatModifier(collector, stat, ctx);
			}
		}
	}

	public bool HasModifiersForStat(StatType stat)
	{
		List<RegisteredGlobalModifier> list = _modifiersByStat[(int)stat];
		if (list != null)
		{
			return list.Count > 0;
		}
		return false;
	}

	public bool HasFullyConditionalModifier(StatType stat)
	{
		List<RegisteredGlobalModifier> list = _modifiersByStat[(int)stat];
		if (list == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsConditional && list[i].DependsOnStats == null)
			{
				return true;
			}
		}
		return false;
	}

	public List<RegisteredGlobalModifier>? GetModifiersForStat(StatType stat)
	{
		return _modifiersByStat[(int)stat];
	}

	public static void Clear()
	{
		_Instance = null;
	}
}
