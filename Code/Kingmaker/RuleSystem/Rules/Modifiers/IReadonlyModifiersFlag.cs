namespace Kingmaker.RuleSystem.Rules.Modifiers;

public interface IReadonlyModifiersFlag : IReadonlyModifiers
{
	bool Value { get; }
}
