using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.Framework.Mechanics.Actor;

public static class BuiltInStatModifiers
{
	public delegate void ModifierApply(StatModifierCollector collector, StatType stat, StatContext context);

	private static List<ModifierApply>?[] _Modifiers = Array.Empty<List<ModifierApply>>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Initialize()
	{
		_Modifiers = new List<ModifierApply>[StatTypeHelper.AllStatsArraySize];
	}

	public static void Register(StatType stat, ModifierApply apply)
	{
		if (stat < StatType.Unknown || (int)stat >= _Modifiers.Length)
		{
			throw new ArgumentOutOfRangeException("stat", $"StatType {stat} ({(int)stat}) out of range [0..{_Modifiers.Length})");
		}
		List<ModifierApply>[] modifiers = _Modifiers;
		if (modifiers[(int)stat] == null)
		{
			modifiers[(int)stat] = new List<ModifierApply>();
		}
		_Modifiers[(int)stat].Add(apply);
	}

	public static void Apply(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat < StatType.Unknown || (int)stat >= _Modifiers.Length)
		{
			return;
		}
		List<ModifierApply> list = _Modifiers[(int)stat];
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i](collector, stat, context);
			}
		}
	}

	public static bool HasModifier(StatType stat)
	{
		if (stat < StatType.Unknown || (int)stat >= _Modifiers.Length)
		{
			return false;
		}
		List<ModifierApply> list = _Modifiers[(int)stat];
		if (list != null)
		{
			return list.Count > 0;
		}
		return false;
	}
}
