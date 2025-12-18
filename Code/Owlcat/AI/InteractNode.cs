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
		InteractionAction value = m_Interactable.Value;
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

	private bool TryCreateInteractCommand(BaseUnitEntity agent, InteractionAction interaction, out UnitCommandParams commandParams)
	{
		commandParams = null;
		InteractionActionPart interactionActionPart = interaction.EnsurePart();
		if (interactionActionPart.Type == InteractionType.Flashlight || interactionActionPart.Type == InteractionType.Variant)
		{
			return false;
		}
		if (!interactionActionPart.CanInteract())
		{
			return false;
		}
		if (interactionActionPart.Type == InteractionType.Direct)
		{
			commandParams = UnitDirectInteract.CreateCommandParams(interactionActionPart);
			return true;
		}
		if (interactionActionPart.Type == InteractionType.Approach && interactionActionPart.IsEnoughCloseForInteraction(agent))
		{
			commandParams = new UnitInteractWithObjectParams(interactionActionPart)
			{
				IsSynchronized = true
			};
			return true;
		}
		return false;
	}
}
