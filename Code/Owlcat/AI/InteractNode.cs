using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class InteractNode : TaskNode
{
	private readonly EntityVariable m_Agent;

	private readonly InteractableVariable m_Interactable;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	private UnitCommandHandle m_CommandHandle;

	private UnitCommandParams m_CommandParams;

	public InteractNode(EntityVariable agent, InteractableVariable interactable, AiAgentRuntimeInternalDataVariable runtimeInternalData)
	{
		m_Agent = agent;
		m_Interactable = interactable;
		m_RuntimeInternalData = runtimeInternalData;
	}

	protected override NodeResult OnRunningTick()
	{
		BaseUnitEntity baseUnitEntity = m_Agent.Value as BaseUnitEntity;
		InteractionActionPart value = m_Interactable.Value;
		if (m_CommandParams == null && !TryCreateInteractCommand(baseUnitEntity, value, out m_CommandParams))
		{
			return NodeResult.Failure;
		}
		m_RuntimeInternalData.Value.ResetIdleTime();
		if (m_CommandHandle == null)
		{
			if (!baseUnitEntity.Commands.Empty)
			{
				return NodeResult.Running;
			}
			m_CommandHandle = baseUnitEntity.Commands.RunImmediate(m_CommandParams);
			if (m_CommandHandle != null)
			{
				return NodeResult.Running;
			}
			return NodeResult.Failure;
		}
		AbstractUnitCommand cmd = m_CommandHandle.Cmd;
		if (cmd != null && !cmd.IsFinished)
		{
			return NodeResult.Running;
		}
		m_CommandHandle = null;
		m_CommandParams = null;
		m_RuntimeInternalData.Value.Invalidate();
		return NodeResult.Success;
	}

	private bool TryCreateInteractCommand(BaseUnitEntity agent, InteractionActionPart interactionPart, out UnitCommandParams commandParams)
	{
		commandParams = null;
		if (interactionPart.Type == InteractionType.Flashlight || interactionPart.Type == InteractionType.Variant)
		{
			return false;
		}
		if (!interactionPart.CanInteract())
		{
			return false;
		}
		if (interactionPart.Type == InteractionType.Direct)
		{
			commandParams = UnitDirectInteract.CreateCommandParams(interactionPart);
			return true;
		}
		if (interactionPart.Type == InteractionType.Approach && interactionPart.IsEnoughCloseForInteraction(agent))
		{
			commandParams = new UnitInteractWithObjectParams(interactionPart)
			{
				IsSynchronized = true
			};
			return true;
		}
		return false;
	}
}
