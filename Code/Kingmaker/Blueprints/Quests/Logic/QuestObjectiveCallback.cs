using System.Text;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[TypeId("c3f36a2e95828334e9f701ba86c781ba")]
public class QuestObjectiveCallback : QuestObjectiveComponentDelegate, IQuestObjectiveCallback
{
	[SerializeField]
	private ActionList m_OnComplete;

	[SerializeField]
	private ActionList m_OnFail;

	void IQuestObjectiveCallback.OnComplete()
	{
		m_OnComplete.Run();
	}

	void IQuestObjectiveCallback.OnFail()
	{
		m_OnFail.Run();
	}

	public override string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (m_OnComplete.HasActions)
		{
			stringBuilder.Append("On Complete ");
			stringBuilder.Append(ElementsDescription.Actions(true, m_OnComplete));
		}
		if (m_OnFail.HasActions)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("\n");
			}
			stringBuilder.Append("On Fail ");
			stringBuilder.Append(ElementsDescription.Actions(true, m_OnFail));
		}
		return stringBuilder.ToString();
	}
}
