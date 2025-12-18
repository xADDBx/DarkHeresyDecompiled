using System;
using Kingmaker.Blueprints;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.EntityBlackboard;

namespace Owlcat.BehaviourTrees;

public class EncounterBlackboardVariable : BlackboardVariable<RuntimeEntityBlackboard>
{
	private readonly BlueprintEncounter m_Encounter;

	private RuntimeEntityBlackboard m_CachedValue;

	public BlackboardVariablesList VariablesList { get; }

	public override RuntimeEntityBlackboard Value
	{
		get
		{
			if (m_Encounter == null)
			{
				return null;
			}
			if (m_CachedValue == null && ActiveEncounter.Current?.Blueprint == m_Encounter)
			{
				m_CachedValue = ActiveEncounter.Current.Blackboard;
			}
			return m_CachedValue;
		}
		set
		{
			throw new Exception("Can't set value of EncounterBlackboardVariable");
		}
	}

	public EncounterBlackboardVariable(BlueprintEncounter encounter)
	{
		m_Encounter = encounter;
		if (m_Encounter == null || !m_Encounter.TryGetComponent<EntityBlackboardComponent>(out var component))
		{
			throw new Exception("Can't create EncounterBlackboardVariable");
		}
		VariablesList = component.Variables;
	}
}
