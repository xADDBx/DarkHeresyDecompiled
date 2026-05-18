using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests.Logic.CrusadeQuests;
using Kingmaker.Code.Gameplay.Enums.Stats;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests;

[TypeId("eddc7c1e81adf5144acd394ebc24ca92")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintQuest : BlueprintFact, IEditorCommentHolder
{
	public class SilentQuestNotificationOverride : IDisposable
	{
		[CanBeNull]
		private readonly BlueprintQuest m_Quest;

		private readonly QuestNotificationState m_OriginalState;

		public SilentQuestNotificationOverride([CanBeNull] BlueprintQuest quest, QuestNotificationState stateOverride)
		{
			m_Quest = quest;
			if (m_Quest != null)
			{
				m_OriginalState = m_Quest.m_SilentQuestNotification;
				m_Quest.m_SilentQuestNotification = stateOverride;
			}
		}

		public void Dispose()
		{
			if (m_Quest != null)
			{
				m_Quest.m_SilentQuestNotification = m_OriginalState;
			}
		}
	}

	[NotNull]
	public LocalizedString Description;

	[NotNull]
	public LocalizedString Title;

	[NotNull]
	public LocalizedString CompletionText;

	[NotNull]
	public LocalizedString RegionName;

	[NotNull]
	public LocalizedString Place;

	[NotNull]
	public LocalizedString ServiceMessage;

	[SerializeField]
	private QuestGroupId m_Group;

	[SerializeField]
	private int m_DescriptionPriority;

	[SerializeField]
	private QuestType m_Type;

	[SerializeField]
	private int m_LastChapter;

	[SerializeField]
	[HideInInspector]
	private List<BlueprintQuestObjectiveReference> m_Objectives = new List<BlueprintQuestObjectiveReference>();

	[SerializeField]
	private int m_CompletionExperience;

	[SerializeField]
	[ShowIf("m_IsRumour")]
	private bool m_IsRumourFalse;

	[SerializeField]
	private QuestNotificationState m_SilentQuestNotification;

	[HideInInspector]
	[SerializeField]
	private EditorCommentHolder m_EditorComment;

	[InfoBox("Этот блок настроек - meta информация для статистики")]
	public Chapter Chapter;

	public Cluster Cluster;

	private bool m_IsRumour => false;

	public EditorCommentHolder EditorComment { get; set; }

	public IEnumerable<BlueprintQuestObjective> AllObjectives => m_Objectives.Select((BlueprintQuestObjectiveReference o) => o.Get());

	public IEnumerable<BlueprintQuestObjective> Addendums
	{
		get
		{
			foreach (BlueprintQuestObjectiveReference objective in m_Objectives)
			{
				foreach (BlueprintQuestObjective addendum in objective.Get().Addendums)
				{
					yield return addendum;
				}
			}
		}
	}

	public IEnumerable<BlueprintQuestObjective> Objectives
	{
		get
		{
			foreach (BlueprintQuestObjectiveReference objective in m_Objectives)
			{
				if (objective != null)
				{
					BlueprintQuestObjective blueprintQuestObjective = objective.Get();
					if (blueprintQuestObjective != null && !blueprintQuestObjective.IsAddendum)
					{
						yield return objective.Get();
					}
				}
			}
		}
	}

	public QuestGroupId Group => m_Group;

	public int DescriptionPriority => m_DescriptionPriority;

	public QuestType Type => m_Type;

	public int LastChapter => m_LastChapter;

	public bool IsRumourFalse => m_IsRumourFalse;

	public int CompletionExperience => m_CompletionExperience;

	public string GetDescription()
	{
		QuestDescriptionModifier component = this.GetComponent<QuestDescriptionModifier>();
		if (component == null)
		{
			return Description;
		}
		return component.Modify(Description);
	}

	public static Quest CreateNewQuest(BlueprintQuest quest)
	{
		return new Quest(quest);
	}

	public bool IsSilentQuestNotification(QuestNotificationState state)
	{
		return (m_SilentQuestNotification & state) != 0;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		UpdateAndValidateObjectives();
	}

	protected override Type GetFactType()
	{
		return typeof(Quest);
	}

	private void RemoveNullReferences()
	{
		int num = m_Objectives.RemoveAll((BlueprintQuestObjectiveReference objective) => objective == null);
		if (num > 0)
		{
			PFLog.Quests.Warning(this, "Quest '{0}': removed {1} missing objectives", this, num);
		}
	}

	private void UpdateObjectives()
	{
		m_Objectives.ForEach(delegate(BlueprintQuestObjectiveReference objective)
		{
			objective.Get().RemoveNullReferences();
			objective.Get().SetQuest(this);
			if (m_Type == QuestType.Errand && !objective.Get().IsHidden)
			{
				objective.Get().SetIsFinishParent(finishParent: true);
			}
		});
		List<BlueprintQuestObjectiveReference> list = m_Objectives.ToList();
		foreach (BlueprintQuestObjective addendum in Addendums)
		{
			if (addendum != null)
			{
				addendum.SetIsAddendum(isAddendum: true);
				list.RemoveAll((BlueprintQuestObjectiveReference r) => r.Is(addendum));
			}
		}
		foreach (BlueprintQuestObjectiveReference item in list)
		{
			item.Get().SetIsAddendum(isAddendum: false);
		}
		m_Objectives.ForEach(delegate(BlueprintQuestObjectiveReference objective)
		{
			objective.Get().Addendums.ForEach(delegate(BlueprintQuestObjective addendum)
			{
				addendum?.SetIsAddendum(isAddendum: true);
			});
		});
		SetDirty();
	}

	public void UpdateAndValidateObjectives()
	{
		RemoveNullReferences();
		UpdateObjectives();
		SetDirty();
	}

	public void LinkObjective(BlueprintQuestObjective objective)
	{
		if (m_Type == QuestType.Errand)
		{
			foreach (BlueprintQuestObjectiveReference objective2 in m_Objectives)
			{
				if (!objective2.Get().IsHidden)
				{
					objective.IsHidden = true;
					break;
				}
			}
		}
		if (objective.Quest != this || !m_Objectives.HasItem((BlueprintQuestObjectiveReference r) => r.Is(objective)))
		{
			if (objective.Quest != null)
			{
				PFLog.Quests.Warning("Objective already linked to quest, link changed");
				objective.Quest.UnlinkObjective(objective);
			}
			m_Objectives.Add(objective.ToReference<BlueprintQuestObjectiveReference>());
			objective.SetQuest(this);
			SetDirty();
		}
	}

	public void UnlinkObjective(BlueprintQuestObjective objective)
	{
		if (!m_Objectives.HasItem((BlueprintQuestObjectiveReference r) => r.Is(objective)))
		{
			PFLog.Quests.Warning("Quest doesn't contains this objective");
		}
		m_Objectives.RemoveAll((BlueprintQuestObjectiveReference r) => r.Is(objective));
		objective.SetQuest(null);
		SetDirty();
	}

	public void AddObjectiveFromMenu(object userdata)
	{
		LinkObjective((BlueprintQuestObjective)userdata);
	}
}
