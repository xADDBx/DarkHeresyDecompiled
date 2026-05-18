using System.Collections;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public sealed class StatModifiersManager : CompositeModifiersManager, IReadonlyModifiersStat, IReadonlyModifiersComposite, IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	private readonly ModifiableValue _stat;

	private readonly List<Modifier> _displayModifiers = new List<Modifier>();

	private bool _dirty;

	protected override bool KeepNonStackingModifiers => true;

	public StatModifiersManager(ModifiableValue stat)
	{
		_stat = stat;
	}

	protected override void OnAdded(Modifier modifier)
	{
		_dirty = true;
		using (ProfileScope.New("Adding modifier"))
		{
			Sort();
		}
	}

	protected override void OnRemoved(Modifier modifier)
	{
		_dirty = true;
		using (ProfileScope.New("Removing modifier"))
		{
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
