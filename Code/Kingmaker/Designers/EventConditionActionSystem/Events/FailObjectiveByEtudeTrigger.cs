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
[TypeId("60df93db482a47b19ebb4153181b2204")]
[ComponentName("Events/FailObjectiveByEtudeTrigger")]
public class FailObjectiveByEtudeTrigger : QuestObjectiveComponentDelegate, IEtudesUpdateHandler, ISubscriber
{
	[SerializeField]
	private BlueprintEtudeReference? m_Etude;

	public void OnEtudesUpdate()
	{
		if (m_Etude != null && base.Objective.State != QuestObjectiveState.Failed && Game.Instance.EtudesSystem.EtudeIsCompleted(m_Etude.Get()))
		{
			base.Objective.Fail();
		}
	}

	public override string GetDescription()
	{
		return "Completes objective when etude " + m_Etude?.NameSafe() + " is complete";
	}
}
