using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities;

namespace Owlcat.BehaviourTrees;

public class AbilityVariable : BlackboardVariable<AbilityData>
{
	[CanBeNull]
	public override AbilityData Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.ToString() ?? "<null>");
	}
}
