namespace Owlcat.BehaviourTrees;

public class IsSetEntityPassNode : ConditionPassNode
{
	private readonly EntityVariable m_Entity;

	private readonly bool m_Invert;

	public IsSetEntityPassNode(AbortType abortType, EntityVariable entity, bool invert)
		: base(abortType)
	{
		bool invert2 = invert;
		m_Entity = entity;
		m_Invert = invert2;
	}

	public override bool IsPassed()
	{
		return (m_Entity.Value != null) ^ m_Invert;
	}
}
