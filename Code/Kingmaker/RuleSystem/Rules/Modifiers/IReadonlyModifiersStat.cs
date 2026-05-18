using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiersStat : IReadonlyModifiersComposite, IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	IEnumerable<Modifier> GetDisplayModifiers();
}
