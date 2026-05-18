using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class UseAbilityOnEntityNode : TaskNode
{
	private readonly AbilityVariable m_Ability;

	private readonly EntityVariable m_Agent;

	private readonly EntityVariable m_Target;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	private UnitCommandHandle m_CommandHandle;

	private UnitUseAbilityParams m_CommandParams;

	public UseAbilityOnEntityNode(EntityVariable agent, EntityVariable target, AbilityVariable ability, AiAgentRuntimeInternalDataVariable runtimeInternalData)
	{
		m_Agent = agent;
		m_Target = target;
		m_Ability = ability;
		m_RuntimeInternalData = runtimeInternalData;
	}

	protected override NodeResult OnRunningTick()
	{
		BaseUnitEntity baseUnitEntity = m_Agent.Value as BaseUnitEntity;
		MechanicEntity value = m_Target.Value;
		AbilityData value2 = m_Ability.Value;
		if (m_CommandParams == null && !TryCreateAttackCommand(value, value2, out m_CommandParams))
		{
			return NodeResult.Failure;
		}
		if (m_CommandHandle == null)
		{
			if (!baseUnitEntity.Commands.Empty)
			{
				m_RuntimeInternalData.Value.ResetIdleTime();
				return NodeResult.Running;
			}
			m_CommandHandle = baseUnitEntity.Commands.RunImmediate(m_CommandParams);
			if (m_CommandHandle == null)
			{
				PFLog.AI.Log($"Failed to run UnitUseAbility command: {value2}");
				return NodeResult.Failure;
			}
			m_RuntimeInternalData.Value.ResetIdleTime();
			return NodeResult.Running;
		}
		AbstractUnitCommand cmd = m_CommandHandle.Cmd;
		if (cmd != null && !cmd.IsFinished)
		{
			m_RuntimeInternalData.Value.ResetIdleTime();
			return NodeResult.Running;
		}
		m_CommandHandle = null;
		m_CommandParams = null;
		m_RuntimeInternalData.Value.Invalidate();
		return NodeResult.Success;
	}

	private bool TryCreateAttackCommand(MechanicEntity target, AbilityData ability, out UnitUseAbilityParams commandParams)
	{
		commandParams = null;
		if (target == null)
		{
			PFLog.AI.Log("Failed to cast ability: " + (ability?.ToString() ?? "<null>") + ", target is <null>");
			return false;
		}
		TargetWrapper targetWrapper = new TargetWrapper(target);
		AbilityData.UnavailabilityReasonType? unavailableReason = AbilityData.UnavailabilityReasonType.None;
		if (ability == null || !ability.IsAvailable || !ability.CanTarget(targetWrapper, out unavailableReason))
		{
			PFLog.AI.Log(string.Format("Failed to cast ability: {0} on target: {1} ({2}), reason: {3}", ability?.ToString() ?? "<null>", targetWrapper, targetWrapper.NearestNode, unavailableReason?.ToString()));
			return false;
		}
		AbilityTargetingCache.Instance.SetInactive();
		PFLog.AI.Log($"Cast {ability} on {targetWrapper} ({targetWrapper.NearestNode})");
		commandParams = new UnitUseAbilityParams(ability, targetWrapper);
		return true;
	}
}
