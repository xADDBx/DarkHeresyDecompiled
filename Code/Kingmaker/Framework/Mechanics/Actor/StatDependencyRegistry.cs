using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.Framework.Mechanics.Actor;

public static class StatDependencyRegistry
{
	public delegate void DependencyApply(StatModifierCollector collector, int baseStatValue, int rawBase, MechanicEntity entity, StatType baseStat);

	public readonly struct StatDependencyInfo
	{
		public readonly StatType BaseStat;

		public readonly DependencyApply Apply;

		public StatDependencyInfo(StatType baseStat, DependencyApply apply)
		{
			BaseStat = baseStat;
			Apply = apply;
		}
	}

	private static StatDependencyInfo?[] _Dependencies = Array.Empty<StatDependencyInfo?>();

	private static List<StatType>?[]? _ReverseDeps;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Initialize()
	{
		_Dependencies = new StatDependencyInfo?[StatTypeHelper.AllStatsArraySize];
		_ReverseDeps = null;
	}

	public static void Register(StatType stat, StatType baseStat, DependencyApply apply)
	{
		if (stat < StatType.Unknown || (int)stat >= _Dependencies.Length)
		{
			throw new ArgumentOutOfRangeException("stat", $"StatType {stat} ({(int)stat}) out of range [0..{_Dependencies.Length})");
		}
		_Dependencies[(int)stat] = new StatDependencyInfo(baseStat, apply);
	}

	public static StatDependencyInfo? Get(StatType stat)
	{
		if (stat < StatType.Unknown || (int)stat >= _Dependencies.Length)
		{
			return null;
		}
		return _Dependencies[(int)stat];
	}

	public static void BuildReverseDependencies()
	{
		_ReverseDeps = new List<StatType>[StatTypeHelper.AllStatsArraySize];
		for (int i = 0; i < _Dependencies.Length; i++)
		{
			StatDependencyInfo? statDependencyInfo = _Dependencies[i];
			if (statDependencyInfo.HasValue)
			{
				int baseStat = (int)statDependencyInfo.Value.BaseStat;
				List<StatType>[] reverseDeps = _ReverseDeps;
				int num = baseStat;
				if (reverseDeps[num] == null)
				{
					reverseDeps[num] = new List<StatType>();
				}
				_ReverseDeps[baseStat].Add((StatType)i);
			}
		}
	}

	public static IReadOnlyList<StatType>? GetDependents(StatType baseStat)
	{
		if (_ReverseDeps == null || baseStat < StatType.Unknown || (int)baseStat >= _ReverseDeps.Length)
		{
			return null;
		}
		return _ReverseDeps[(int)baseStat];
	}
}
