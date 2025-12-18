using System.Linq;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueSkill : ModifiableValueDependent<ModifiableValueAttributeStat>
{
	public bool HasPenalties => base.Modifiers.Any((Modifier m) => !m.Permanent && m.Value < 0);

	public bool HasBonuses => base.Modifiers.Any((Modifier m) => !m.Permanent && m.Value > 0);
}
