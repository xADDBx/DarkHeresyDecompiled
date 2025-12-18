using System;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("1134f49faea24114f83d852de3bafd5f")]
public class TutorialTriggerContractStarted : TutorialTrigger, IQuestHandler, ISubscriber
{
	public void HandleQuestStarted(Quest quest)
	{
		if (quest.Blueprint.Type == QuestType.Order)
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
