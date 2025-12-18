using System;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Quests.Logic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Quests.Common;

[Obsolete]
[ComponentName("Common/GiveUnlockOnObjectiveTrigger")]
[TypeId("0f64686a34fa2144ab54c43c2b7ededf")]
public class GiveUnlockOnObjectiveTrigger : QuestObjectiveComponentDelegate, IQuestObjectiveLogic, IUnlockableFlagReference
{
	public QuestObjectiveState objectiveState;

	[SerializeField]
	[FormerlySerializedAs("unlock")]
	private BlueprintUnlockableFlagReference m_unlock;

	public BlueprintUnlockableFlag unlock => m_unlock?.Get();

	void IQuestObjectiveLogic.OnStarted()
	{
		if (objectiveState == QuestObjectiveState.Started)
		{
			unlock.Unlock();
		}
	}

	void IQuestObjectiveLogic.OnCompleted()
	{
		if (objectiveState == QuestObjectiveState.Completed)
		{
			unlock.Unlock();
		}
	}

	void IQuestObjectiveLogic.OnFailed()
	{
		if (objectiveState == QuestObjectiveState.Failed)
		{
			unlock.Unlock();
		}
	}

	void IQuestObjectiveLogic.OnBecameVisible()
	{
	}

	public override string GetDescription()
	{
		return $"Unlock Flag {unlock} when {objectiveState}";
	}

	UnlockableFlagReferenceType IUnlockableFlagReference.GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (unlock == flag)
		{
			return UnlockableFlagReferenceType.Unlock;
		}
		return UnlockableFlagReferenceType.None;
	}
}
