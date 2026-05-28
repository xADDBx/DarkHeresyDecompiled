namespace Owlcat.BehaviourTrees;

public class RandomResultNode : ConditionNode
{
	private readonly float m_Chance;

	public RandomResultNode(AbortType abortType, float chance)
		: base(abortType)
	{
		m_Chance = chance;
	}

	public override bool IsPassed()
	{
		return BehaviourTreeRandomProvider.value <= m_Chance;
	}
}
