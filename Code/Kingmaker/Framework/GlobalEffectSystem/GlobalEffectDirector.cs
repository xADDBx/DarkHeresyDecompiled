using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine.Pool;

namespace Kingmaker.Framework.GlobalEffectSystem;

public sealed class GlobalEffectDirector
{
	public readonly struct WeightEntry
	{
		public readonly Func<float> WeightGetter;

		public readonly int Priority;

		public readonly WeightEntrySource Source;

		public WeightEntry(Func<float> weightGetter, int priority, WeightEntrySource source)
		{
			WeightGetter = weightGetter;
			Priority = priority;
			Source = source;
		}

		public bool IsFrom(WeightEntrySource source)
		{
			return Source.Equals(source);
		}
	}

	public readonly struct WeightEntrySource : IEquatable<WeightEntrySource>
	{
		public static readonly WeightEntrySource Scene = new WeightEntrySource(SourceType.Scene);

		public static readonly WeightEntrySource Code = new WeightEntrySource(SourceType.Code);

		public static readonly WeightEntrySource Cheat = new WeightEntrySource(SourceType.Cheat);

		public readonly SourceType Type;

		public readonly EntityFactRef SourceFact;

		public readonly BlueprintComponent? SourceComponent;

		public readonly CommandBase? SourceCommand;

		public int? FixedPriority => Type switch
		{
			SourceType.Scene => int.MinValue, 
			SourceType.Code => -2147483647, 
			SourceType.Designer => null, 
			SourceType.Cheat => int.MaxValue, 
			_ => throw new ArgumentOutOfRangeException(), 
		};

		private WeightEntrySource(EntityFactComponent source)
			: this(SourceType.Designer)
		{
			SourceFact = source.Fact;
			SourceComponent = source.SourceBlueprintComponent;
		}

		private WeightEntrySource(CommandBase source)
			: this(SourceType.Designer)
		{
			SourceCommand = source;
		}

		private WeightEntrySource(SourceType type)
		{
			Type = type;
			SourceFact = default(EntityFactRef);
			SourceComponent = null;
			SourceCommand = null;
		}

		public bool Equals(WeightEntrySource other)
		{
			if (Type == other.Type && SourceFact.Equals(other.SourceFact) && object.Equals(SourceComponent, other.SourceComponent))
			{
				return object.Equals(SourceCommand, other.SourceCommand);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is WeightEntrySource other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, SourceFact, SourceComponent, SourceCommand);
		}

		public static implicit operator WeightEntrySource(EntityFactComponent source)
		{
			return new WeightEntrySource(source);
		}

		public static implicit operator WeightEntrySource(CommandBase source)
		{
			return new WeightEntrySource(source);
		}
	}

	public enum SourceType
	{
		Scene,
		Code,
		Designer,
		Cheat
	}

	private const int ScenePriority = int.MinValue;

	private const int CodePriority = -2147483647;

	private const int CheatPriority = int.MaxValue;

	private readonly Dictionary<BlueprintGlobalEffect, List<WeightEntry>> _effects = new Dictionary<BlueprintGlobalEffect, List<WeightEntry>>();

	public static GlobalEffectDirector Shared { get; } = new GlobalEffectDirector();


	public IReadOnlyDictionary<BlueprintGlobalEffect, List<WeightEntry>> Effects => _effects;

	public bool Exists(BlueprintGlobalEffect effect)
	{
		return _effects.ContainsKey(effect);
	}

	public void Reset()
	{
		_effects.Clear();
	}

	public float GetWeight(BlueprintGlobalEffect effect)
	{
		if (!_effects.TryGetValue(effect, out List<WeightEntry> value))
		{
			throw new KeyNotFoundException("Global Effect not found: " + effect.name);
		}
		List<WeightEntry> list = value;
		return Math.Clamp(list[list.Count - 1].WeightGetter(), 0f, 1f);
	}

	public void SetWeightFromDesigner(BlueprintGlobalEffect effect, float weight, int priority, EntityFactComponent source)
	{
		Set(effect, () => weight, source, priority);
	}

	public void SetWeightFromDesigner(BlueprintGlobalEffect effect, float weight, int priority, CommandBase source)
	{
		Set(effect, () => weight, source, priority);
	}

	public void RemoveWeightFromDesigner(EntityFactComponent source)
	{
		RemoveWeightFromDesigner((WeightEntrySource)source);
	}

	public void RemoveWeightFromDesigner(CommandBase source)
	{
		RemoveWeightFromDesigner((WeightEntrySource)source);
	}

	public void SetWeightFromCode(BlueprintGlobalEffect effect, Func<float> weightGetter)
	{
		Set(effect, weightGetter, WeightEntrySource.Code);
	}

	public void SetWeightFromCode(BlueprintGlobalEffect effect, float weight)
	{
		SetWeightFromCode(effect, () => weight);
	}

	public void RemoveWeightFromCode(BlueprintGlobalEffect effect)
	{
		RemoveWeightFromDesigner(effect, WeightEntrySource.Code);
	}

	public void SetWeightFromCheat(BlueprintGlobalEffect effect, float weight)
	{
		Set(effect, () => weight, WeightEntrySource.Cheat);
	}

	public void RemoveWeightFromCheat(BlueprintGlobalEffect effect)
	{
		RemoveWeightFromDesigner(effect, WeightEntrySource.Cheat);
	}

	public void SetWeightFromScene(BlueprintGlobalEffect effect, Func<float> weightGetter)
	{
		Set(effect, weightGetter, WeightEntrySource.Scene);
	}

	public void RemoveWeightFromScene(BlueprintGlobalEffect effect)
	{
		RemoveWeightFromDesigner(effect, WeightEntrySource.Scene);
	}

	private void Set(BlueprintGlobalEffect effect, Func<float> weightGetter, WeightEntrySource source, int priority = 0)
	{
		if (effect == null)
		{
			throw new ArgumentNullException("effect");
		}
		int? fixedPriority = source.FixedPriority;
		priority = fixedPriority ?? priority;
		if (!fixedPriority.HasValue && (priority <= -2147483647 || priority >= int.MaxValue))
		{
			throw new ArgumentOutOfRangeException("priority");
		}
		if (!_effects.TryGetValue(effect, out List<WeightEntry> value))
		{
			value = (_effects[effect] = new List<WeightEntry>());
		}
		if (!fixedPriority.HasValue && value.Any((WeightEntry i) => i.IsFrom(source)))
		{
			throw new InvalidOperationException("Duplicate weight entry from the same source");
		}
		WeightEntry weightEntry = new WeightEntry(weightGetter, priority, source);
		if (fixedPriority.HasValue)
		{
			int num = value.FindIndex((WeightEntry i) => i.Priority == priority);
			if (num >= 0)
			{
				value[num] = weightEntry;
				return;
			}
		}
		for (int j = 0; j < value.Count; j++)
		{
			WeightEntry weightEntry2 = value[j];
			if (priority < weightEntry2.Priority)
			{
				value.Insert(j, weightEntry);
				return;
			}
		}
		value.Add(weightEntry);
	}

	private void RemoveWeightFromDesigner(WeightEntrySource source)
	{
		List<BlueprintGlobalEffect> value;
		using (CollectionPool<List<BlueprintGlobalEffect>, BlueprintGlobalEffect>.Get(out value))
		{
			foreach (var (item, list2) in _effects)
			{
				list2.RemoveAll((WeightEntry i) => i.IsFrom(source));
				if (list2.Count == 0)
				{
					value.Add(item);
				}
			}
			foreach (BlueprintGlobalEffect item2 in value)
			{
				_effects.Remove(item2);
			}
		}
	}

	private void RemoveWeightFromDesigner(BlueprintGlobalEffect effect, WeightEntrySource source)
	{
		if (_effects.TryGetValue(effect, out List<WeightEntry> value))
		{
			value.RemoveAll((WeightEntry i) => i.IsFrom(source));
			if (value.Count == 0)
			{
				_effects.Remove(effect);
			}
		}
	}
}
