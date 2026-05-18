using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine.Pool;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public sealed class PredictionHackContext : ContextData<PredictionHackContext>
{
	private static readonly Lazy<ObjectPool<CompositeModifiersManager>> ModifiersManagerPool = new Lazy<ObjectPool<CompositeModifiersManager>>(new ObjectPool<CompositeModifiersManager>(() => new CompositeModifiersManager(), null, delegate(CompositeModifiersManager manager)
	{
		manager.RemoveAll((Modifier _) => true);
	}));

	[CanBeNull]
	private List<BlueprintFact> _factRanksToIncrement;

	[CanBeNull]
	private Dictionary<StatType, CompositeModifiersManager> _statModifiers;

	public int VeilDeltaBeforeCast;

	public List<BlueprintFact> FactRanksToIncrement => _factRanksToIncrement ?? (_factRanksToIncrement = CollectionPool<List<BlueprintFact>, BlueprintFact>.Get());

	public IReadOnlyDictionary<StatType, CompositeModifiersManager> StatModifiers => EnsureStatModifiers();

	public bool HasFactRanksToIncrement => _factRanksToIncrement != null;

	public bool HasStatModifiers => _statModifiers != null;

	public CompositeModifiersManager GetModifiersManager(StatType statType)
	{
		EnsureStatModifiers();
		if (!_statModifiers.TryGetValue(statType, out var value))
		{
			value = (_statModifiers[statType] = ModifiersManagerPool.Value.Get());
		}
		return value;
	}

	protected override void Reset()
	{
		if (_factRanksToIncrement != null)
		{
			CollectionPool<List<BlueprintFact>, BlueprintFact>.Release(_factRanksToIncrement);
			_factRanksToIncrement = null;
		}
		if (_statModifiers != null)
		{
			foreach (CompositeModifiersManager value in _statModifiers.Values)
			{
				ModifiersManagerPool.Value.Release(value);
			}
			CollectionPool<Dictionary<StatType, CompositeModifiersManager>, KeyValuePair<StatType, CompositeModifiersManager>>.Release(_statModifiers);
			_statModifiers = null;
		}
		VeilDeltaBeforeCast = 0;
	}

	private Dictionary<StatType, CompositeModifiersManager> EnsureStatModifiers()
	{
		return _statModifiers ?? (_statModifiers = CollectionPool<Dictionary<StatType, CompositeModifiersManager>, KeyValuePair<StatType, CompositeModifiersManager>>.Get());
	}
}
