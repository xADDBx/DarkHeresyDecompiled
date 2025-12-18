using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Code.View.UI.Test;

public class NotificationsTest : MonoBehaviour
{
	[SerializeField]
	private BpRef<BlueprintQuest> m_TestQuest;

	private Quest m_CurrentQuest;

	[ContextMenu("Start Quest")]
	private void StartQuest()
	{
		m_CurrentQuest = BlueprintQuest.CreateNewQuest(m_TestQuest);
		EventBus.RaiseEvent(delegate(IQuestHandler h)
		{
			h.HandleQuestStarted(m_CurrentQuest);
		});
	}

	[ContextMenu("Close Quest")]
	private void CloseQuest()
	{
		if (m_CurrentQuest != null)
		{
			EventBus.RaiseEvent(delegate(IQuestHandler h)
			{
				h.HandleQuestCompleted(m_CurrentQuest);
			});
		}
	}
}
