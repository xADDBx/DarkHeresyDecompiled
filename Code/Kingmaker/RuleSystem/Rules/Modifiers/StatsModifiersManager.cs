using System.Collections.Generic;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public sealed class StatsModifiersManager : CompositeModifiersManager
{
	private readonly ModifiableValue _stat;

	private readonly List<Modifier> _displayModifiers = new List<Modifier>();

	private bool _dirty;

	protected override bool KeepNonStackingModifiers => true;

	public StatsModifiersManager(ModifiableValue stat)
	{
		_stat = stat;
	}

	protected override void OnAdded(Modifier modifier)
	{
		_dirty = true;
		using (ProfileScope.New("Adding modifier"))
		{
			Sort();
			_stat.Owner?.GetOptional<UnitPartNonStackBonuses>()?.HandleModifierAdded(_stat, modifier);
		}
	}

	protected override void OnRemoved(Modifier modifier)
	{
		_dirty = true;
		using (ProfileScope.New("Removing modifier"))
		{
			_stat.Owner?.GetOptional<UnitPartNonStackBonuses>()?.HandleModifierRemoving(_stat, modifier);
		}
	}

	public new bool Add(Modifier modifier)
	{
		return TryAdd(modifier);
	}

	public IEnumerable<Modifier> GetDisplayModifiers()
	{
		UpdateCache();
		return _displayModifiers;
	}

	private void UpdateCache()
	{
		if (!_dirty)
		{
			return;
		}
		_displayModifiers.Clear();
		Visit(delegate(Modifier m, Status s)
		{
			if (s == Status.Enabled)
			{
				_displayModifiers.Add(m);
			}
		});
		_dirty = false;
	}
}
