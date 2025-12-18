using System.Collections.Generic;
using System.Linq;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueAttributeStat : ModifiableValue
{
	private HashSet<EntityFactComponent>? _disableSources;

	public int Damage
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public int Drain
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool HasPenalties => base.Modifiers.Any((Modifier m) => !m.Permanent && m.Value < 0);

	public bool HasBonuses => base.Modifiers.Any((Modifier m) => !m.Permanent && m.Value > 0);

	public bool Enabled
	{
		get
		{
			HashSet<EntityFactComponent> disableSources = _disableSources;
			return disableSources == null || disableSources.Count <= 0;
		}
	}

	protected override int MinValue => 1;

	public int Bonus => base.ModifiedValue / 10;

	public int WarhammerBonus => base.ModifiedValue / 10;

	public int PermanentBonus => base.PermanentValue / 10;

	protected override void UpdateInternalModifiers()
	{
		base.UpdateInternalModifiers();
		if (!Enabled)
		{
			AddInternalModifier(ModifierType.PctMul_Extra, 0);
		}
	}

	public void Disable(EntityFactComponent source)
	{
		(_disableSources ?? (_disableSources = new HashSet<EntityFactComponent>())).Add(source);
		UpdateValue();
	}

	public void Enable(EntityFactComponent source)
	{
		_disableSources?.Remove(source);
		UpdateValue();
	}
}
