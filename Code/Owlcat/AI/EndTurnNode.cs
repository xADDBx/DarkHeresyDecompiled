using Kingmaker.EntitySystem.Entities;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class EndTurnNode : ActionNode
{
	private readonly EntityVariable m_Agent;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	public EndTurnNode(EntityVariable agent, AiAgentRuntimeInternalDataVariable runtimeInternalData)
	{
		m_Agent = agent;
		m_RuntimeInternalData = runtimeInternalData;
	}

	protected override void DoAction()
	{
		BaseUnitEntity baseUnitEntity = m_Agent.Value as BaseUnitEntity;
		AiAgentRuntimeInternalData value = m_RuntimeInternalData.Value;
		if (!value.EndTurnRequest && baseUnitEntity.Commands.Empty)
		{
			value.EndTurnRequest = true;
		}
	}
}
