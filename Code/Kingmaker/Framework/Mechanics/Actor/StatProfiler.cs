using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.Cheats;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine;

namespace Kingmaker.Framework.Mechanics.Actor;

public static class StatProfiler
{
	public readonly struct Snapshot
	{
		public readonly int CallCount;

		public readonly int CacheHits;

		public readonly int CacheMisses;

		public readonly int AgainstCount;

		public readonly int PropertiesCount;

		public readonly int OutputCount;

		public readonly int FilterCount;

		public readonly double TotalMs;

		public readonly int InvalidationCount;

		public readonly int InvalidateAllCount;

		public readonly int[]? StatHistogram;

		public readonly Dictionary<string, int>? CallerHistogram;

		public readonly int[]? InvalidationHistogram;

		public readonly Dictionary<string, int>? InvalidationCallerHistogram;

		public Snapshot(int callCount, int cacheHits, int cacheMisses, int againstCount, int propertiesCount, int outputCount, int filterCount, double totalMs, int invalidationCount, int invalidateAllCount, int[]? statHistogram, Dictionary<string, int>? callerHistogram, int[]? invalidationHistogram, Dictionary<string, int>? invalidationCallerHistogram)
		{
			CallCount = callCount;
			CacheHits = cacheHits;
			CacheMisses = cacheMisses;
			AgainstCount = againstCount;
			PropertiesCount = propertiesCount;
			OutputCount = outputCount;
			FilterCount = filterCount;
			TotalMs = totalMs;
			InvalidationCount = invalidationCount;
			InvalidateAllCount = invalidateAllCount;
			StatHistogram = statHistogram;
			CallerHistogram = callerHistogram;
			InvalidationHistogram = invalidationHistogram;
			InvalidationCallerHistogram = invalidationCallerHistogram;
		}
	}

	[ThreadStatic]
	private static int _CacheHits;

	[ThreadStatic]
	private static int _CacheMisses;

	[ThreadStatic]
	private static int _AgainstCount;

	[ThreadStatic]
	private static int _PropertiesCount;

	[ThreadStatic]
	private static int _OutputCount;

	[ThreadStatic]
	private static int _FilterCount;

	[ThreadStatic]
	private static long _TotalTicks;

	[ThreadStatic]
	private static int[]? _StatHistogram;

	[ThreadStatic]
	private static Dictionary<string, int>? _CallerHistogram;

	[ThreadStatic]
	private static int _InvalidationCount;

	[ThreadStatic]
	private static int _InvalidateAllCount;

	[ThreadStatic]
	private static int[]? _InvalidationHistogram;

	[ThreadStatic]
	private static Dictionary<string, int>? _InvalidationCallerHistogram;

	public static bool Enabled { get; private set; }

	[Cheat(Name = "stats_profiler_arm", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string Arm()
	{
		Reset();
		Enabled = true;
		return "Stats profiler armed. Trigger the action you want to profile, then run `stats_profiler_dump`.";
	}

	[Cheat(Name = "stats_profiler_dump", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string Dump()
	{
		if (!Enabled)
		{
			return "Stats profiler is not armed. Run `stats_profiler_arm` first.";
		}
		Snapshot snapshot = Flush();
		Enabled = false;
		string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "stats-debug"));
		Directory.CreateDirectory(fullPath);
		string text = Path.Combine(fullPath, $"StatProfilerDump-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
		using StreamWriter streamWriter = new StreamWriter(text);
		streamWriter.WriteLine($"# Stats profiler dump — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
		streamWriter.WriteLine($"# CallCount (root)      : {snapshot.CallCount}");
		streamWriter.WriteLine($"# CacheHits  (root)     : {snapshot.CacheHits}");
		streamWriter.WriteLine($"# CacheMisses (root)    : {snapshot.CacheMisses}");
		streamWriter.WriteLine($"# TotalMs (root)        : {snapshot.TotalMs:F2}");
		streamWriter.WriteLine($"# ctx.Against != null   : {snapshot.AgainstCount}");
		streamWriter.WriteLine($"# ctx.HasProperties     : {snapshot.PropertiesCount}");
		streamWriter.WriteLine($"# output != null        : {snapshot.OutputCount}");
		streamWriter.WriteLine($"# filter != null        : {snapshot.FilterCount}");
		streamWriter.WriteLine($"# Invalidation calls    : {snapshot.InvalidationCount}");
		streamWriter.WriteLine($"# InvalidateAll calls   : {snapshot.InvalidateAllCount}");
		streamWriter.WriteLine();
		streamWriter.WriteLine("== Top stats by call count ==");
		if (snapshot.StatHistogram != null)
		{
			foreach (var item in (from p in snapshot.StatHistogram.Select((int c, int i) => (idx: i, count: c))
				where p.count > 0
				orderby p.count descending
				select p).Take(40))
			{
				streamWriter.WriteLine($"  {item.count,8}  {(StatType)item.idx}");
			}
		}
		streamWriter.WriteLine();
		streamWriter.WriteLine("== Top callers by call count ==");
		if (snapshot.CallerHistogram != null)
		{
			foreach (KeyValuePair<string, int> item2 in snapshot.CallerHistogram.OrderByDescending<KeyValuePair<string, int>, int>((KeyValuePair<string, int> kv) => kv.Value).Take(40))
			{
				streamWriter.WriteLine($"  {item2.Value,8}  {item2.Key}");
			}
		}
		streamWriter.WriteLine();
		streamWriter.WriteLine("== Top stats by INVALIDATION count (cache busts per stat) ==");
		if (snapshot.InvalidationHistogram != null)
		{
			foreach (var item3 in (from p in snapshot.InvalidationHistogram.Select((int c, int i) => (idx: i, count: c))
				where p.count > 0
				orderby p.count descending
				select p).Take(40))
			{
				streamWriter.WriteLine($"  {item3.count,8}  {(StatType)item3.idx}");
			}
		}
		streamWriter.WriteLine();
		streamWriter.WriteLine("== Top INVALIDATION callers (who notifies stat changes) ==");
		if (snapshot.InvalidationCallerHistogram != null)
		{
			foreach (KeyValuePair<string, int> item4 in snapshot.InvalidationCallerHistogram.OrderByDescending<KeyValuePair<string, int>, int>((KeyValuePair<string, int> kv) => kv.Value).Take(40))
			{
				streamWriter.WriteLine($"  {item4.Value,8}  {item4.Key}");
			}
		}
		return $"Stats profiler dump → {text}\n  calls={snapshot.CallCount}  hits={snapshot.CacheHits}  misses={snapshot.CacheMisses}  ms={snapshot.TotalMs:F1}  invalidations={snapshot.InvalidationCount}";
	}

	[Cheat(Name = "stat_cacheability_dump", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string DumpCacheability()
	{
		EntityPools entityPools = Game.Instance?.EntityPools;
		if (entityPools == null)
		{
			return "Game.Instance.EntityPools is null — load a save / area first.";
		}
		string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "stats-debug"));
		Directory.CreateDirectory(fullPath);
		string text = Path.Combine(fullPath, $"StatCacheabilityDump-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
		int num = 0;
		int num2 = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		List<string> list = new List<string>();
		StatType[] allStats;
		foreach (BaseUnitEntity allBaseUnit in entityPools.AllBaseUnits)
		{
			if (allBaseUnit == null || allBaseUnit.Actor == null)
			{
				continue;
			}
			num++;
			List<StatType> list2 = new List<StatType>();
			allStats = StatTypeHelper.AllStats;
			foreach (StatType statType in allStats)
			{
				if (!allBaseUnit.Actor.IsStatCacheable(statType))
				{
					IReadOnlyList<MechanicActor.RegisteredModifier> registeredModifiers = allBaseUnit.Actor.GetRegisteredModifiers(statType);
					if (registeredModifiers != null && registeredModifiers.Count != 0)
					{
						list2.Add(statType);
					}
				}
			}
			list.Add("== Unit: " + DescribeUnit(allBaseUnit) + " ==");
			if (list2.Count == 0)
			{
				list.Add("  (no non-cacheable stats with registered modifiers)");
				list.Add(string.Empty);
				continue;
			}
			list.Add($"  Non-cacheable stats ({list2.Count}):");
			foreach (StatType item in list2)
			{
				IReadOnlyList<MechanicActor.RegisteredModifier>? registeredModifiers2 = allBaseUnit.Actor.GetRegisteredModifiers(item);
				list.Add($"    {item}");
				foreach (MechanicActor.RegisteredModifier item2 in registeredModifiers2)
				{
					bool flag = item2.IsConditional && item2.DependsOnStats == null;
					string text2 = (flag ? "[BREAKS]" : "        ");
					string text3 = item2.Modifier?.GetType().Name ?? item2.Component?.GetType().Name ?? "?";
					string text4 = SafeFactName(item2.Component);
					string text5 = ((item2.DependsOnStats == null) ? "null" : ("[" + string.Join(",", item2.DependsOnStats) + "]"));
					list.Add($"      {text2} Component={text3}  fact={text4}  IsConditional={item2.IsConditional}  DependsOnStats={text5}  Scope={item2.Scope}");
					if (flag)
					{
						dictionary[text3] = ((!dictionary.TryGetValue(text3, out var value)) ? 1 : (value + 1));
					}
				}
				num2++;
			}
			list.Add(string.Empty);
		}
		List<string> list3 = new List<string> { "== Global registry ==" };
		GlobalStatModifierRegistry instance = GlobalStatModifierRegistry.Instance;
		int num3 = 0;
		allStats = StatTypeHelper.AllStats;
		foreach (StatType statType2 in allStats)
		{
			List<GlobalStatModifierRegistry.RegisteredGlobalModifier> modifiersForStat = instance.GetModifiersForStat(statType2);
			if (modifiersForStat == null || modifiersForStat.Count == 0)
			{
				continue;
			}
			list3.Add($"  {statType2}:");
			foreach (GlobalStatModifierRegistry.RegisteredGlobalModifier item3 in modifiersForStat)
			{
				string text6 = ((item3.IsConditional && item3.DependsOnStats == null) ? "[BREAKS-ALL]" : "            ");
				string text7 = item3.Modifier?.GetType().Name ?? item3.Component?.GetType().Name ?? "?";
				string text8 = SafeFactName(item3.Component);
				string text9 = ((item3.DependsOnStats == null) ? "null" : ("[" + string.Join(",", item3.DependsOnStats) + "]"));
				list3.Add($"    {text6} Component={text7}  fact={text8}  IsConditional={item3.IsConditional}  DependsOnStats={text9}");
				num3++;
			}
		}
		if (num3 == 0)
		{
			list3.Add("  (empty)");
		}
		using (StreamWriter streamWriter = new StreamWriter(text))
		{
			streamWriter.WriteLine($"# Stat cacheability dump — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			streamWriter.WriteLine($"# Units inspected: {num}");
			streamWriter.WriteLine($"# Non-cacheable stat-instances (unit,stat pairs): {num2}");
			streamWriter.WriteLine("# Top components breaking entity-level cache:");
			foreach (KeyValuePair<string, int> item4 in dictionary.OrderByDescending((KeyValuePair<string, int> kv) => kv.Value).Take(20))
			{
				streamWriter.WriteLine($"#   {item4.Value,5} × {item4.Key}");
			}
			streamWriter.WriteLine();
			foreach (string item5 in list3)
			{
				streamWriter.WriteLine(item5);
			}
			streamWriter.WriteLine();
			foreach (string item6 in list)
			{
				streamWriter.WriteLine(item6);
			}
		}
		return $"Stat cacheability dump → {text}\n  units={num}  non_cacheable_stat_instances={num2}";
	}

	private static string DescribeUnit(BaseUnitEntity unit)
	{
		string text = unit.CharacterName;
		if (string.IsNullOrEmpty(text))
		{
			text = unit.Blueprint?.name ?? "?";
		}
		return text + " [eid=" + unit.UniqueId + ", blueprint=" + (unit.Blueprint?.name ?? "?") + "]";
	}

	private static string SafeFactName(EntityFactComponent? component)
	{
		try
		{
			return component?.Fact?.Blueprint?.name ?? "?";
		}
		catch
		{
			return "?";
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnCall(StatType stat, StatQueryOutput? output, in StatContext ctx, Func<Modifier, bool>? filter, string? caller)
	{
		if (Enabled)
		{
			RecordCall(stat, output, in ctx, filter, caller);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnCacheHit(bool isRoot)
	{
		if (Enabled && isRoot)
		{
			_CacheHits++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnCacheMiss(bool isRoot)
	{
		if (Enabled && isRoot)
		{
			_CacheMisses++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnRootCallTime(long elapsedTicks)
	{
		if (Enabled)
		{
			_TotalTicks += elapsedTicks;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnInvalidation(ulong mask, string? caller, bool invalidateAll = false)
	{
		if (Enabled)
		{
			RecordInvalidation(mask, caller, invalidateAll);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void RecordCall(StatType stat, StatQueryOutput? output, in StatContext ctx, Func<Modifier, bool>? filter, string? caller)
	{
		if (_StatHistogram == null)
		{
			_StatHistogram = new int[StatTypeHelper.AllStatsArraySize];
		}
		_StatHistogram[(int)stat]++;
		if (caller != null)
		{
			if (_CallerHistogram == null)
			{
				_CallerHistogram = new Dictionary<string, int>();
			}
			_CallerHistogram[caller] = ((!_CallerHistogram.TryGetValue(caller, out var value)) ? 1 : (value + 1));
		}
		if (ctx.Against != null)
		{
			_AgainstCount++;
		}
		if (ctx.HasProperties)
		{
			_PropertiesCount++;
		}
		if (output != null)
		{
			_OutputCount++;
		}
		if (filter != null)
		{
			_FilterCount++;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void RecordInvalidation(ulong mask, string? caller, bool invalidateAll)
	{
		_InvalidationCount++;
		if (invalidateAll)
		{
			_InvalidateAllCount++;
		}
		if (_InvalidationHistogram == null)
		{
			_InvalidationHistogram = new int[StatTypeHelper.AllStatsArraySize];
		}
		for (int i = 0; i < StatTypeHelper.AllStatsArraySize; i++)
		{
			if ((mask & (ulong)(1L << i)) != 0L)
			{
				_InvalidationHistogram[i]++;
			}
		}
		if (caller != null)
		{
			if (_InvalidationCallerHistogram == null)
			{
				_InvalidationCallerHistogram = new Dictionary<string, int>();
			}
			_InvalidationCallerHistogram[caller] = ((!_InvalidationCallerHistogram.TryGetValue(caller, out var value)) ? 1 : (value + 1));
		}
	}

	public static void Reset()
	{
		_CacheHits = 0;
		_CacheMisses = 0;
		_AgainstCount = 0;
		_PropertiesCount = 0;
		_OutputCount = 0;
		_FilterCount = 0;
		_TotalTicks = 0L;
		_StatHistogram = null;
		_CallerHistogram = null;
		_InvalidationCount = 0;
		_InvalidateAllCount = 0;
		_InvalidationHistogram = null;
		_InvalidationCallerHistogram = null;
	}

	public static Snapshot Flush()
	{
		int callCount = _CacheHits + _CacheMisses;
		Snapshot result = new Snapshot(callCount, _CacheHits, _CacheMisses, _AgainstCount, _PropertiesCount, _OutputCount, _FilterCount, (double)_TotalTicks * 1000.0 / (double)Stopwatch.Frequency, _InvalidationCount, _InvalidateAllCount, _StatHistogram, _CallerHistogram, _InvalidationHistogram, _InvalidationCallerHistogram);
		Reset();
		return result;
	}
}
