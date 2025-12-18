using System;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiers
{
	ReadonlyList<Modifier> List { get; }

	Modifier? GetModifier(Func<Modifier, bool> pred);
}
