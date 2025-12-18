using System;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiersValue : IReadonlyModifiers
{
	int Value { get; }

	int GetValue(Func<Modifier, bool>? filter = null);
}
