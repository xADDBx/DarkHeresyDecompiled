using System;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("37b3a50d387d984438733f2ea7c9ca2c")]
public class IsRumourFalse : Condition
{
	[SerializeField]
	private BlueprintQuestReference m_Rumour;

	protected override string GetConditionCaption()
	{
		return "Check if player complete rumour and it is false";
	}

	protected override bool CheckCondition()
	{
		if (m_Rumour.Get().IsRumourFalse)
		{
			return Game.Instance.QuestBook.GetQuestState(m_Rumour.Get()) == QuestState.Completed;
		}
		return false;
	}
}
