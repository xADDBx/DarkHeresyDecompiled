namespace Owlcat.BehaviourTrees;

public class AreSameEntitiesPassNode : ConditionPassNode
{
	private readonly EntityVariable m_Entity1;

	private readonly EntityVariable m_Entity2;

	private readonly bool m_Invert;

	public AreSameEntitiesPassNode(AbortType abortType, EntityVariable entity1, EntityVariable entity2, bool invert)
		: base(abortType)
	{
		m_Entity1 = entity1;
		m_Entity2 = entity2;
		m_Invert = invert;
	}

	public override bool IsPassed()
	{
		return (m_Entity1.Value == m_Entity2.Value) ^ m_Invert;
	}
}
