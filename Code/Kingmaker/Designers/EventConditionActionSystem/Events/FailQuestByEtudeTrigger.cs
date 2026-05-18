using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("785928ab10ac40b19a015b8e444261a9")]
[ComponentName("Events/FailQuestByEtudeTrigger")]
public class FailQuestByEtudeTrigger : QuestComponentDelegate, IEtudesUpdateHandler, ISubscriber
{
	[SerializeField]
	private BlueprintEtudeReference? m_Etude;

	[SerializeField]
	private bool m_FailByPlay = true;

	[SerializeField]
	private bool m_FailByComplete;

	[SerializeField]
	private bool m_FailSilently = true;

	public void OnEtudesUpdate()
	{
		if (m_Etude != null && base.Quest.State != QuestState.Failed)
		{
			if (m_FailByPlay && Game.Instance.EtudesSystem.EtudeIsPlaying(m_Etude.Get()))
			{
				base.Quest.FailQuest(m_FailSilently);
			}
			else if (m_FailByComplete && Game.Instance.EtudesSystem.EtudeIsCompleted(m_Etude.Get()))
			{
				base.Quest.FailQuest(m_FailSilently);
			}
		}
	}
}
