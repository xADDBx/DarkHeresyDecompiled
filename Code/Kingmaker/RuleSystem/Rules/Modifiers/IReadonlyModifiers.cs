using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiers : IEnumerable<Modifier>, IEnumerable
{
	ReadonlyList<Modifier> List { get; }

	Modifier? GetModifier(Func<Modifier, bool> pred);
}
