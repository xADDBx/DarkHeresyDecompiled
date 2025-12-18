namespace Owlcat.BehaviourTrees;

public class AbilityAvailabilityConditionNode : ConditionNode
{
	private readonly AbilityVariable m_Ability;

	private readonly bool m_Invert;

	public AbilityAvailabilityConditionNode(AbortType abortType, AbilityVariable ability, bool invert)
		: base(abortType)
	{
		m_Ability = ability;
		m_Invert = invert;
	}

	public override bool IsPassed()
	{
		return (m_Ability.Value?.IsAvailable ?? false) ^ m_Invert;
	}
}
