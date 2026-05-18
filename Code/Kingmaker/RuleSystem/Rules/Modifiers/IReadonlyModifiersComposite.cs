using System;
using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiersComposite : IReadonlyModifiers, IEnumerable<Modifier>, IEnumerable
{
	int Value { get; }

	IEnumerable<Modifier> ValueModifiersList { get; }

	IEnumerable<Modifier> PercentModifiersList { get; }

	IEnumerable<Modifier> PercentMultipliersList { get; }

	IEnumerable<Modifier> ValueModifiersExtraList { get; }

	IEnumerable<Modifier> PercentMultipliersExtraList { get; }

	int Apply(int value, Func<Modifier, bool>? filter = null);

	int ApplyPctMulExtra(int value);

	void GetValues(out int valAdd, out float pctAdd, out float pctMul, out int valAddExtra, out float pctMulExtra, Func<Modifier, bool>? filter = null);

	bool Contains(Func<Modifier, bool> pred);
}
