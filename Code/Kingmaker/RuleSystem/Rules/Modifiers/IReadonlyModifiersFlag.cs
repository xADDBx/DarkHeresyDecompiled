using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiersFlag : IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	bool Value { get; }
}
