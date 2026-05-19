using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Utility.CodeTimer;
using UnityEngine.Pool;

namespace Kingmaker.Framework.Mechanics.Actor;

public sealed class MechanicActor
{
	internal readonly struct RegisteredModifier
	{
		public readonly EntityFactComponent Component;

		public readonly IStatModifier Modifier;

		public readonly bool IsConditional;

		public readonly StatType[]? DependsOnStats;

		public readonly StatModifierScope Scope;

		public RegisteredModifier(EntityFactComponent component, IStatModifier modifier, bool isConditional, StatType[]? dependsOnStats, StatModifierScope scope)
		{
			Component = component;
			Modifier = modifier;
			IsConditional = isConditional;
			DependsOnStats = dependsOnStats;
			Scope = scope;
		}
	}

	private const int MaxRecursionDepth = 8;

	private const string GetStatProfilerName = "GetStat";

	[ThreadStatic]
	private static int _recursionDepth;

	[ThreadStatic]
	private static StatType[]? _statStack;

	[ThreadStatic]
	private static int _frameCallCount;

	[ThreadStatic]
	private static long _frameTotalTicks;

	private readonly MechanicEntity _entity;

	private readonly List<RegisteredModifier>?[] _modifiersByStat;

	private readonly StatResult[] _cachedStatResults;

	private readonly bool[] _cacheValid;

	private readonly bool[] _isCacheable;

	private readonly HashSet<StatType>?[] _conditionalDependencies;

	private readonly StatBaseValue[] _cachedStatBaseValues;

	private int?[]? _cheatValues;

	private int[]? _disableCounts;

	private ulong _visitedStats;

	private bool _baseValuesFilled;

	public MechanicEntity Entity => _entity;

	public List<EntityFact> Facts => _entity.Facts.List;

	public MechanicActorStatsCollection Stats => new MechanicActorStatsCollection(this);

	[UsedImplicitly]
	public IEnumerable<MechanicActorStat> AllStats
	{
		get
		{
			StatType[] allStats = StatTypeHelper.AllStats;
			foreach (StatType type in allStats)
			{
				yield return new MechanicActorStat(this, type);
			}
		}
	}

	private static string FormatCycleError(StatType failedStat)
	{
		StatType[] statStack = _statStack;
		int recursionDepth = _recursionDepth;
		int num = -1;
		if (statStack != null)
		{
			for (int i = 0; i < recursionDepth && i < 8; i++)
			{
				if (statStack[i] == failedStat)
				{
					num = i;
					break;
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append($"MechanicActor.GetStat: cycle detected for {failedStat}.");
		if (num >= 0)
		{
			stringBuilder.Append(" Chain: ");
			for (int j = num; j < recursionDepth && j < 8; j++)
			{
				stringBuilder.Append(statStack[j]);
				stringBuilder.Append(" → ");
			}
			stringBuilder.Append(failedStat);
		}
		return stringBuilder.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool ScanFullOverrides(StatModifierCollector collector, StatType stat, StatContext ctx, out StatType? deferredOverride, out int deferredOverrideValue)
	{
		deferredOverride = null;
		deferredOverrideValue = int.MinValue;
		if (collector.FullOverrides.Count == 0)
		{
			return false;
		}
		for (int num = collector.FullOverrides.Count - 1; num >= 0; num--)
		{
			StatOverrideEntry statOverrideEntry = collector.FullOverrides[num];
			if (!statOverrideEntry.OnlyIfHigher)
			{
				deferredOverride = statOverrideEntry.Stat;
				return true;
			}
			int statValue = GetStatValue(statOverrideEntry.Stat, ctx);
			if (statValue > deferredOverrideValue)
			{
				deferredOverrideValue = statValue;
				deferredOverride = statOverrideEntry.Stat;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private StatType? ResolveDependency(StatModifierCollector collector, StatType stat, StatContext ctx, int rawBase)
	{
		StatDependencyRegistry.StatDependencyInfo? statDependencyInfo = StatDependencyRegistry.Get(stat);
		if (!statDependencyInfo.HasValue)
		{
			return null;
		}
		StatType effectiveBaseStat = statDependencyInfo.Value.BaseStat;
		StatType? baseStat = effectiveBaseStat;
		if (collector.BaseOverrides.Count > 0)
		{
			ResolveBaseOverrides(collector, ctx, ref effectiveBaseStat, ref baseStat);
		}
		int statValue = GetStatValue(effectiveBaseStat, ctx);
		statDependencyInfo.Value.Apply(collector, statValue, rawBase, _entity, effectiveBaseStat);
		return baseStat;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ResolveBaseOverrides(StatModifierCollector collector, StatContext ctx, ref StatType effectiveBaseStat, ref StatType? baseStat)
	{
		int statValue = GetStatValue(effectiveBaseStat, ctx);
		StatType? statType = null;
		int num = int.MinValue;
		for (int num2 = collector.BaseOverrides.Count - 1; num2 >= 0; num2--)
		{
			StatOverrideEntry statOverrideEntry = collector.BaseOverrides[num2];
			if (!statOverrideEntry.OnlyIfHigher)
			{
				effectiveBaseStat = statOverrideEntry.Stat;
				baseStat = statOverrideEntry.Stat;
				return;
			}
			int statValue2 = GetStatValue(statOverrideEntry.Stat, ctx);
			if (statValue2 > num)
			{
				num = statValue2;
				statType = statOverrideEntry.Stat;
			}
		}
		if (statType.HasValue && num > statValue)
		{
			effectiveBaseStat = statType.Value;
			baseStat = statType;
		}
	}

	internal void HandleFactActivated(EntityFact fact)
	{
		List<AffectedStatEntry> value;
		using (CollectionPool<List<AffectedStatEntry>, AffectedStatEntry>.Get(out value))
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			foreach (EntityFactComponent component in fact.Components)
			{
				if (!(component.SourceBlueprintComponent is IStatModifier statModifier))
				{
					continue;
				}
				value.Clear();
				using (component.SetScope())
				{
					statModifier.CollectAffectedStats(value);
					StatModifierScope scope = statModifier.Scope;
					foreach (AffectedStatEntry item in value)
					{
						if (scope == StatModifierScope.Global)
						{
							GlobalStatModifierRegistry.Instance.Register(component, statModifier, item.Stat, item.IsConditional, item.DependsOnStats);
							num2 |= (ulong)(1L << (int)item.Stat);
							continue;
						}
						List<RegisteredModifier>[] modifiersByStat = _modifiersByStat;
						int stat = (int)item.Stat;
						(modifiersByStat[stat] ?? (modifiersByStat[stat] = new List<RegisteredModifier>())).Add(new RegisteredModifier(component, statModifier, item.IsConditional, item.DependsOnStats, scope));
						if (item.IsConditional && item.DependsOnStats == null)
						{
							_isCacheable[(int)item.Stat] = false;
						}
						if (item.IsConditional)
						{
							if (item.DependsOnStats != null)
							{
								StatType[] dependsOnStats = item.DependsOnStats;
								foreach (StatType num3 in dependsOnStats)
								{
									HashSet<StatType>[] conditionalDependencies = _conditionalDependencies;
									int num4 = (int)num3;
									(conditionalDependencies[num4] ?? (conditionalDependencies[num4] = new HashSet<StatType>())).Add(item.Stat);
								}
								num |= (ulong)(1L << (int)item.Stat);
							}
						}
						else
						{
							num |= (ulong)(1L << (int)item.Stat);
						}
					}
				}
			}
			if (num != 0L)
			{
				NotifyStatsChanged(num, "HandleFactActivated");
			}
			if (num2 != 0L)
			{
				NotifyAllEntitiesStatsChanged(num2);
			}
		}
	}

	internal void HandleFactDeactivated(EntityFact fact)
	{
		List<AffectedStatEntry> value;
		using (CollectionPool<List<AffectedStatEntry>, AffectedStatEntry>.Get(out value))
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			foreach (EntityFactComponent component in fact.Components)
			{
				if (!(component.SourceBlueprintComponent is IStatModifier statModifier))
				{
					continue;
				}
				value.Clear();
				using (component.SetScope())
				{
					statModifier.CollectAffectedStats(value);
					StatModifierScope scope = statModifier.Scope;
					foreach (AffectedStatEntry item in value)
					{
						if (scope == StatModifierScope.Global)
						{
							GlobalStatModifierRegistry.Instance.Unregister(component, item.Stat);
							num2 |= (ulong)(1L << (int)item.Stat);
							continue;
						}
						List<RegisteredModifier> list = _modifiersByStat[(int)item.Stat];
						if (list != null)
						{
							for (int num3 = list.Count - 1; num3 >= 0; num3--)
							{
								if (list[num3].Component == component)
								{
									list.RemoveAt(num3);
									break;
								}
							}
						}
						if (item.IsConditional && item.DependsOnStats == null)
						{
							_isCacheable[(int)item.Stat] = !HasFullyConditionalModifier((int)item.Stat);
						}
						if (item.DependsOnStats != null)
						{
							StatType[] dependsOnStats = item.DependsOnStats;
							foreach (StatType depStat in dependsOnStats)
							{
								RecomputeConditionalDependency(depStat);
							}
						}
						if (!item.IsConditional || item.DependsOnStats != null)
						{
							num |= (ulong)(1L << (int)item.Stat);
						}
					}
				}
			}
			if (num != 0L)
			{
				NotifyStatsChanged(num, "HandleFactDeactivated");
			}
			if (num2 != 0L)
			{
				NotifyAllEntitiesStatsChanged(num2);
			}
		}
	}

	private void RegisterFactModifiers(EntityFact fact)
	{
		List<AffectedStatEntry> value;
		using (CollectionPool<List<AffectedStatEntry>, AffectedStatEntry>.Get(out value))
		{
			foreach (EntityFactComponent component in fact.Components)
			{
				if (!(component.SourceBlueprintComponent is IStatModifier statModifier))
				{
					continue;
				}
				value.Clear();
				using (component.SetScope())
				{
					statModifier.CollectAffectedStats(value);
					StatModifierScope scope = statModifier.Scope;
					foreach (AffectedStatEntry item in value)
					{
						if (scope == StatModifierScope.Global)
						{
							GlobalStatModifierRegistry.Instance.Register(component, statModifier, item.Stat, item.IsConditional, item.DependsOnStats);
							continue;
						}
						List<RegisteredModifier>[] modifiersByStat = _modifiersByStat;
						int stat = (int)item.Stat;
						(modifiersByStat[stat] ?? (modifiersByStat[stat] = new List<RegisteredModifier>())).Add(new RegisteredModifier(component, statModifier, item.IsConditional, item.DependsOnStats, scope));
						if (item.IsConditional && item.DependsOnStats != null)
						{
							StatType[] dependsOnStats = item.DependsOnStats;
							foreach (StatType num in dependsOnStats)
							{
								HashSet<StatType>[] conditionalDependencies = _conditionalDependencies;
								int num2 = (int)num;
								(conditionalDependencies[num2] ?? (conditionalDependencies[num2] = new HashSet<StatType>())).Add(item.Stat);
							}
						}
					}
				}
			}
		}
	}

	private static void NotifyAllEntitiesStatsChanged(ulong changedStatsMask)
	{
		Game instance = Game.Instance;
		if (instance?.EntityPools == null)
		{
			return;
		}
		foreach (MechanicEntity mechanicEntity in instance.EntityPools.MechanicEntities)
		{
			MechanicActor actor = mechanicEntity.Actor;
			if (!(actor == null))
			{
				actor.NotifyStatsChanged(changedStatsMask, "NotifyAllEntitiesStatsChanged");
			}
		}
	}

	private void RecomputeConditionalDependency(StatType depStat)
	{
		HashSet<StatType> hashSet = null;
		for (int i = 0; i < _modifiersByStat.Length; i++)
		{
			List<RegisteredModifier> list = _modifiersByStat[i];
			if (list == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				RegisteredModifier registeredModifier = list[j];
				if (registeredModifier.DependsOnStats == null)
				{
					continue;
				}
				for (int k = 0; k < registeredModifier.DependsOnStats.Length; k++)
				{
					if (registeredModifier.DependsOnStats[k] == depStat)
					{
						if (hashSet == null)
						{
							hashSet = new HashSet<StatType>();
						}
						hashSet.Add((StatType)i);
						break;
					}
				}
			}
		}
		_conditionalDependencies[(int)depStat] = hashSet;
	}

	public void InvalidateAllStatCaches([CallerMemberName] string? caller = null)
	{
		StatProfiler.OnInvalidation(ulong.MaxValue, caller, invalidateAll: true);
		Array.Fill(_cacheValid, value: false);
		InvalidateStatBaseValueCache();
		EventBus.RaiseEvent((IMechanicEntity)_entity, (Action<IActorStatChangedHandler>)delegate(IActorStatChangedHandler h)
		{
			h.HandleActorStatChanged(new StatChangeSet(ulong.MaxValue));
		}, isCheckRuntime: true);
	}

	internal void NotifyStatChanged(StatType stat, [CallerMemberName] string? caller = null)
	{
		NotifyStatsChanged((ulong)(1L << (int)stat), caller);
	}

	internal void NotifyStatsChanged(ulong changedStatsMask, [CallerMemberName] string? caller = null)
	{
		ExpandAllDependents(ref changedStatsMask);
		StatProfiler.OnInvalidation(changedStatsMask, caller);
		for (int i = 0; i < StatTypeHelper.AllStatsArraySize; i++)
		{
			if ((changedStatsMask & (ulong)(1L << i)) != 0L)
			{
				_cacheValid[i] = false;
			}
		}
		EventBus.RaiseEvent((IMechanicEntity)_entity, (Action<IActorStatChangedHandler>)delegate(IActorStatChangedHandler h)
		{
			h.HandleActorStatChanged(new StatChangeSet(changedStatsMask));
		}, isCheckRuntime: true);
	}

	private void ExpandAllDependents(ref ulong mask)
	{
		List<StatType> value;
		using (CollectionPool<List<StatType>, StatType>.Get(out value))
		{
			for (int i = 0; i < StatTypeHelper.AllStatsArraySize; i++)
			{
				if ((mask & (ulong)(1L << i)) != 0L)
				{
					value.Add((StatType)i);
				}
			}
			int num = 0;
			while (num < value.Count)
			{
				StatType statType = value[num++];
				IReadOnlyList<StatType> dependents = StatDependencyRegistry.GetDependents(statType);
				if (dependents != null)
				{
					for (int j = 0; j < dependents.Count; j++)
					{
						ulong num2 = (ulong)(1L << (int)dependents[j]);
						if ((mask & num2) == 0L)
						{
							mask |= num2;
							value.Add(dependents[j]);
						}
					}
				}
				HashSet<StatType> hashSet = _conditionalDependencies[(int)statType];
				if (hashSet == null)
				{
					continue;
				}
				foreach (StatType item in hashSet)
				{
					ulong num3 = (ulong)(1L << (int)item);
					if ((mask & num3) == 0L)
					{
						mask |= num3;
						value.Add(item);
					}
				}
			}
		}
	}

	private void InvalidateStatCache(StatType stat)
	{
		ulong mask = (ulong)(1L << (int)stat);
		ExpandAllDependents(ref mask);
		for (int i = 0; i < StatTypeHelper.AllStatsArraySize; i++)
		{
			if ((mask & (ulong)(1L << i)) != 0L)
			{
				_cacheValid[i] = false;
			}
		}
	}

	public MechanicActor(MechanicEntity entity)
	{
		_entity = entity;
		int allStatsArraySize = StatTypeHelper.AllStatsArraySize;
		_modifiersByStat = new List<RegisteredModifier>[allStatsArraySize];
		_cachedStatResults = new StatResult[allStatsArraySize];
		_cacheValid = new bool[allStatsArraySize];
		_conditionalDependencies = new HashSet<StatType>[allStatsArraySize];
		_cachedStatBaseValues = new StatBaseValue[allStatsArraySize];
		_isCacheable = new bool[allStatsArraySize];
		Array.Fill(_isCacheable, value: true);
		if (!entity.IsPostLoadExecuted)
		{
			return;
		}
		foreach (EntityFact item in _entity.Facts.List)
		{
			if (item.IsActive)
			{
				RegisterFactModifiers(item);
			}
		}
		for (int i = 0; i < allStatsArraySize; i++)
		{
			_isCacheable[i] = !HasFullyConditionalModifier(i);
		}
	}

	public static void FlushFrameCounters(out int callCount, out double totalMs)
	{
		callCount = _frameCallCount;
		totalMs = (double)_frameTotalTicks * 1000.0 / (double)Stopwatch.Frequency;
		_frameCallCount = 0;
		_frameTotalTicks = 0L;
	}

	public StatResult GetStat(StatType stat, StatQueryOutput? output = null, StatContext ctx = default(StatContext), [CallerMemberName] string? caller = null)
	{
		StatProfiler.OnCall(stat, output, in ctx, null, caller);
		using (ProfileScope.New("GetStat"))
		{
			return GetStatInternal(stat, null, output, ctx, clamp: true);
		}
	}

	public int GetStatBase(StatType stat)
	{
		return GetCachedStatBaseValue(stat).Value;
	}

	public int GetStatBonus(StatType stat)
	{
		return GetStat(stat, null, default(StatContext), "GetStatBonus").ModifiedValue / 10;
	}

	public int GetStatPermanent(StatType stat)
	{
		using (ProfileScope.New("GetStat"))
		{
			return GetStatInternal(stat, IsPermanentModifier, null, default(StatContext), clamp: true).ModifiedValue;
		}
	}

	public int GetStatPermanentBonus(StatType stat)
	{
		return GetStatPermanent(stat) / 10;
	}

	public int GetStatFiltered(StatType stat, Func<Modifier, bool> filter, StatContext ctx = default(StatContext))
	{
		using (ProfileScope.New("GetStat"))
		{
			return GetStatInternal(stat, filter, null, ctx, clamp: true).ModifiedValue;
		}
	}

	public int GetStatUnclamped(StatType stat, StatContext ctx = default(StatContext))
	{
		using (ProfileScope.New("GetStat"))
		{
			return GetStatInternal(stat, null, null, ctx, clamp: false).ModifiedValue;
		}
	}

	private static bool IsPermanentModifier(Modifier m)
	{
		return m.Permanent;
	}

	public static StatType? GetStatBaseStat(StatType stat)
	{
		return StatDependencyRegistry.Get(stat)?.BaseStat;
	}

	public bool IsStatEnabled(StatType stat)
	{
		return !IsStatDisabled(stat);
	}

	public void SetStatCheat(StatType stat, int? value)
	{
		if (_cheatValues == null)
		{
			_cheatValues = new int?[StatTypeHelper.AllStatsArraySize];
		}
		_cheatValues[(int)stat] = value;
		InvalidateStatCache(stat);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int? GetCheatValue(StatType stat)
	{
		int?[]? cheatValues = _cheatValues;
		if (cheatValues == null)
		{
			return null;
		}
		return cheatValues[(int)stat];
	}

	public bool HasCheatValue(StatType stat)
	{
		return _cheatValues?[(int)stat].HasValue ?? false;
	}

	public void DisableStat(StatType stat)
	{
		if (_disableCounts == null)
		{
			_disableCounts = new int[StatTypeHelper.AllStatsArraySize];
		}
		_disableCounts[(int)stat]++;
		InvalidateStatCache(stat);
	}

	public void EnableStat(StatType stat)
	{
		if (_disableCounts == null || _disableCounts[(int)stat] <= 0)
		{
			PFLog.Default.Error($"MechanicActor.EnableStat({stat}): unbalanced call, counter is already 0");
			return;
		}
		_disableCounts[(int)stat]--;
		InvalidateStatCache(stat);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsStatDisabled(StatType stat)
	{
		if (_disableCounts != null)
		{
			return _disableCounts[(int)stat] > 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private StatBaseValue GetCachedStatBaseValue(StatType stat)
	{
		if (!_baseValuesFilled)
		{
			FillStatBaseValueCache();
		}
		return _cachedStatBaseValues[(int)stat];
	}

	internal void InvalidateStatBaseValueCache()
	{
		_baseValuesFilled = false;
	}

	private void FillStatBaseValueCache()
	{
		StatType[] allStats = StatTypeHelper.AllStats;
		foreach (StatType statType in allStats)
		{
			_cachedStatBaseValues[(int)statType] = _entity.GetStatBaseValue(statType);
		}
		_baseValuesFilled = true;
	}

	private bool HasFullyConditionalModifier(int statIndex)
	{
		List<RegisteredModifier> list = _modifiersByStat[statIndex];
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

	private int GetStatValue(StatType stat, StatContext ctx)
	{
		return GetStatInternal(stat, null, null, ctx, clamp: true).ModifiedValue;
	}

	public int GetStatIgnoringOverride(StatType stat, StatContext ctx = default(StatContext))
	{
		return GetStatInternal(stat, null, null, ctx, clamp: true, followOverride: false).ModifiedValue;
	}

	internal StatResult GetStatWithoutOverride(StatType stat, StatQueryOutput? output = null, StatContext ctx = default(StatContext))
	{
		return GetStatInternal(stat, null, output, ctx, clamp: true, followOverride: false);
	}

	private StatResult GetStatInternal(StatType stat, Func<Modifier, bool>? filter, StatQueryOutput? output, StatContext ctx, bool clamp, bool followOverride = true)
	{
		StatBaseValue cachedStatBaseValue = GetCachedStatBaseValue(stat);
		int? cheatValue = GetCheatValue(stat);
		if (cheatValue.HasValue)
		{
			return new StatResult(cheatValue.Value, cachedStatBaseValue.Value, null, null);
		}
		if (cachedStatBaseValue.Forced)
		{
			return new StatResult(cachedStatBaseValue.Value, cachedStatBaseValue.Value, null, null);
		}
		if (IsStatDisabled(stat))
		{
			return new StatResult(1, cachedStatBaseValue.Value, null, null);
		}
		if (StatTypeMetadata.TryGetReadonlyProvider(stat, out Func<MechanicEntity, int> provider))
		{
			return new StatResult(provider(_entity), 0, null, null);
		}
		bool flag = output == null && clamp && followOverride && filter == null && ctx.Against == null && !ctx.HasProperties && _isCacheable[(int)stat] && !GlobalStatModifierRegistry.Instance.HasFullyConditionalModifier(stat);
		if (flag && _cacheValid[(int)stat])
		{
			StatProfiler.OnCacheHit(_recursionDepth == 0);
			return _cachedStatResults[(int)stat];
		}
		ulong num = (ulong)(1L << (int)stat);
		if ((_visitedStats & num) != 0L)
		{
			PFLog.Default.Error(FormatCycleError(stat));
			return new StatResult(cachedStatBaseValue.Value, cachedStatBaseValue.Value, null, null);
		}
		bool flag2 = _recursionDepth == 0;
		StatProfiler.OnCacheMiss(flag2);
		long num2 = 0L;
		if (flag2)
		{
			_frameCallCount++;
			num2 = Stopwatch.GetTimestamp();
		}
		if (_statStack == null)
		{
			_statStack = new StatType[8];
		}
		if (_recursionDepth < 8)
		{
			_statStack[_recursionDepth] = stat;
		}
		_visitedStats |= num;
		_recursionDepth++;
		try
		{
			ctx = ctx.WithOwner(this);
			StatModifierCollector collector;
			using (StatModifierCollectorPool.Get(out collector))
			{
				CollectModifiers(collector, stat, ctx);
				int value = cachedStatBaseValue.Value;
				StatType? deferredOverride = null;
				int deferredOverrideValue = int.MinValue;
				if (followOverride && ScanFullOverrides(collector, stat, ctx, out deferredOverride, out deferredOverrideValue))
				{
					StatResult statInternal = GetStatInternal(deferredOverride.Value, filter, output, ctx, clamp);
					return new StatResult(statInternal.ModifiedValue, statInternal.BaseValue, statInternal.BaseStat, deferredOverride);
				}
				StatType? baseStat = ResolveDependency(collector, stat, ctx, value);
				int num3 = ((filter != null) ? collector.Modifiers.Apply(value, filter) : collector.Apply(value));
				if (followOverride && deferredOverride.HasValue && deferredOverrideValue > num3)
				{
					StatResult statInternal2 = GetStatInternal(deferredOverride.Value, filter, output, ctx, clamp);
					return new StatResult(statInternal2.ModifiedValue, statInternal2.BaseValue, statInternal2.BaseStat, deferredOverride);
				}
				if (clamp)
				{
					num3 = Math.Max((stat.IsAttribute() || stat == StatType.MaxHitPoints) ? 1 : int.MinValue, num3);
				}
				output?.CopyFrom(collector);
				StatResult statResult = new StatResult(num3, value, baseStat, null);
				if (flag)
				{
					_cachedStatResults[(int)stat] = statResult;
					_cacheValid[(int)stat] = true;
				}
				return statResult;
			}
		}
		finally
		{
			if (flag2)
			{
				long num4 = Stopwatch.GetTimestamp() - num2;
				_frameTotalTicks += num4;
				StatProfiler.OnRootCallTime(num4);
			}
			_recursionDepth--;
			_visitedStats &= ~num;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CollectModifiers(StatModifierCollector collector, StatType stat, StatContext ctx)
	{
		IterateRegisteredModifiers(collector, stat, ctx, StatModifierScope.Owner);
		if (ctx.Against != null)
		{
			ctx.Against.IterateRegisteredModifiers(collector, stat, ctx, StatModifierScope.Against);
		}
		((IStatModifier)_entity.GetOptional<PartEquipmentStats>())?.TryApplyStatModifier(collector, stat, ctx);
		((IStatModifier)_entity.GetOptional<PartArmor>())?.TryApplyStatModifier(collector, stat, ctx);
		GlobalStatModifierRegistry.Instance.IterateModifiers(collector, stat, ctx);
		BuiltInStatModifiers.Apply(collector, stat, ctx);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void IterateRegisteredModifiers(StatModifierCollector collector, StatType stat, StatContext ctx, StatModifierScope requiredScope)
	{
		List<RegisteredModifier> list = _modifiersByStat[(int)stat];
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			RegisteredModifier registeredModifier = list[i];
			if (registeredModifier.Scope == requiredScope)
			{
				using (registeredModifier.Component.SetScope())
				{
					registeredModifier.Modifier.TryApplyStatModifier(collector, stat, ctx);
				}
			}
		}
	}

	public T? GetOptional<T>() where T : EntityPart, new()
	{
		return _entity.GetOptional<T>();
	}

	public T GetRequired<T>() where T : EntityPart, new()
	{
		return _entity.GetRequired<T>();
	}

	public static bool operator ==(MechanicActor? a, MechanicEntity? e)
	{
		return a?._entity == e;
	}

	public static bool operator !=(MechanicActor? a, MechanicEntity? e)
	{
		return a?._entity != e;
	}

	public static bool operator ==(MechanicEntity? e, MechanicActor? a)
	{
		return e == a?._entity;
	}

	public static bool operator !=(MechanicEntity? e, MechanicActor? a)
	{
		return e != a?._entity;
	}

	public override bool Equals(object? obj)
	{
		if (obj is MechanicActor mechanicActor)
		{
			return _entity == mechanicActor._entity;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _entity?.GetHashCode() ?? 0;
	}

	internal bool IsStatCacheable(StatType stat)
	{
		return _isCacheable[(int)stat];
	}

	internal IReadOnlyList<RegisteredModifier>? GetRegisteredModifiers(StatType stat)
	{
		return _modifiersByStat[(int)stat];
	}
}
