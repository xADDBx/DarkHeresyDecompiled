using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("0faf2e62c7ca4e688000ecbc439688f5")]
public class TutorialTriggerRumourStarted : TutorialTrigger, IQuestHandler, ISubscriber
{
	public void HandleQuestStarted(Quest quest)
	{
		if (quest.Blueprint.Type == QuestType.Rumour || quest.Blueprint.Type == QuestType.RumourAboutUs)
		{
			TryToTrigger(null);
		}
	}

	public void HandleQuestCompleted(Quest objective)
	{
	}

	public void HandleQuestFailed(Quest objective)
	{
	}

	public void HandleQuestUpdated(Quest objective)
	{
	}
}
