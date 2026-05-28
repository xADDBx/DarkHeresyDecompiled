namespace Owlcat.BehaviourTrees;

public class FixedResultNode : DecoratorNode
{
	private readonly NodeFixedResult m_Result;

	public FixedResultNode(NodeFixedResult result)
	{
		m_Result = result;
	}

	protected override NodeResult Decorate(NodeResult result)
	{
		if (result == NodeResult.Running)
		{
			return NodeResult.Running;
		}
		if (m_Result != 0)
		{
			return NodeResult.Failure;
		}
		return NodeResult.Success;
	}
}
