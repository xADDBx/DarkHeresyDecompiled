using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Enums;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/QuestsRoot")]
[TypeId("b850b137861f5e1458ce02fc13bbdb06")]
public class QuestsRoot : BlueprintScriptableObject
{
	[SerializeField]
	private BlueprintQuestGroupsReference m_Groups;

	public IEnumerable<QuestGroup> Groups
	{
		get
		{
			if (!m_Groups.IsEmpty())
			{
				return m_Groups.Get().Groups;
			}
			return Enumerable.Empty<QuestGroup>();
		}
	}

	public QuestGroup GetGroup(QuestGroupId groupId)
	{
		QuestGroup questGroup = Groups.FirstOrDefault((QuestGroup g) => g.Id == groupId);
		if (questGroup == null)
		{
			PFLog.Default.Error("Can't find quest group with id '{0}'", groupId);
			questGroup = new QuestGroup();
		}
		return questGroup;
	}
}
