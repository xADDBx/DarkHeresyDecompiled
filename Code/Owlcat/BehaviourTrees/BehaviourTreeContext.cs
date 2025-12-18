using Kingmaker.ElementsSystem.ContextData;

namespace Owlcat.BehaviourTrees;

public class BehaviourTreeContext : ContextData<BehaviourTreeContext>
{
	private Blackboard m_Blackboard;

	public static Blackboard Blackboard => ContextData<BehaviourTreeContext>.Current?.m_Blackboard;

	public BehaviourTreeContext Setup(Blackboard blackboard)
	{
		m_Blackboard = blackboard;
		return this;
	}

	protected override void Reset()
	{
		m_Blackboard = null;
	}
}
