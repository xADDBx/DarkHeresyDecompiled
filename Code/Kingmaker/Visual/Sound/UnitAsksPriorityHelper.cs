namespace Kingmaker.Visual.Sound;

public static class UnitAsksPriorityHelper
{
	private static AskWrapper[] m_BarkWrapperPrioritizationGroups = new AskWrapper[11];

	public static void RegisterBark(AskWrapper wrapper)
	{
		if (wrapper != null)
		{
			int prioritizationGroup = wrapper.Bark.PrioritizationGroup;
			AsksSet bark = wrapper.Bark;
			if (m_BarkWrapperPrioritizationGroups[prioritizationGroup] != null && !m_BarkWrapperPrioritizationGroups[prioritizationGroup].IsPlaying)
			{
				RemoveCurrentHighestPriorityBark(prioritizationGroup);
			}
			if (m_BarkWrapperPrioritizationGroups[prioritizationGroup] == null)
			{
				m_BarkWrapperPrioritizationGroups[prioritizationGroup] = wrapper;
			}
			else if (wrapper != m_BarkWrapperPrioritizationGroups[prioritizationGroup] && bark.Priority < m_BarkWrapperPrioritizationGroups[prioritizationGroup].Bark.Priority)
			{
				m_BarkWrapperPrioritizationGroups[prioritizationGroup].UnitAsksManager.DiscardCurrentActiveBark();
				m_BarkWrapperPrioritizationGroups[prioritizationGroup] = wrapper;
			}
		}
	}

	private static void RemoveCurrentHighestPriorityBark(int prioritizationGroup)
	{
		m_BarkWrapperPrioritizationGroups[prioritizationGroup] = null;
	}

	public static AskWrapper GetCurrentHighestPriorityBark(int prioritizationGroup)
	{
		return m_BarkWrapperPrioritizationGroups[prioritizationGroup];
	}
}
