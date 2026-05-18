using System;
using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiersValue : IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	int Value { get; }

	int GetValue(Func<Modifier, bool>? filter = null);
}
