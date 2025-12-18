using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Quests.Common;

[Obsolete]
[ComponentName("Common/ChangeObjectiveOnUnlockTrigger")]
[AllowMultipleComponents]
[TypeId("2f1196a0ec374f641be1ea83b3010505")]
public class ChangeObjectiveOnUnlockTrigger : QuestObjectiveComponentDelegate, IUnlockHandler, ISubscriber, IQuestObjectiveLogic, IUnlockableFlagReference, IQuestObjectiveReference
{
	public enum ObjectiveStatus
	{
		Start,
		Complete,
		Fail
	}

	public enum UnlockStatus
	{
		OnGain,
		OnLost
	}

	public bool checkUnlockStatusOnStart;

	public ObjectiveStatus setStatus;

	[SerializeField]
	[FormerlySerializedAs("targetObjective")]
	private BlueprintQuestObjectiveReference m_targetObjective;

	[SerializeField]
	[FormerlySerializedAs("unlock")]
	private BlueprintUnlockableFlagReference m_unlock;

	public UnlockStatus unlockStatus;

	public BlueprintQuestObjective targetObjective => m_targetObjective?.Get();

	public BlueprintUnlockableFlag unlock => m_unlock?.Get();

	void IQuestObjectiveLogic.OnStarted()
	{
		if (checkUnlockStatusOnStart)
		{
			if (unlock.IsUnlocked && unlockStatus == UnlockStatus.OnGain)
			{
				SetObjectiveStatus();
			}
			if (!unlock.IsUnlocked && unlockStatus == UnlockStatus.OnLost)
			{
				SetObjectiveStatus();
			}
		}
	}

	void IQuestObjectiveLogic.OnCompleted()
	{
	}

	void IQuestObjectiveLogic.OnFailed()
	{
	}

	void IQuestObjectiveLogic.OnBecameVisible()
	{
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		if (flag == unlock && unlockStatus == UnlockStatus.OnGain)
		{
			SetObjectiveStatus();
		}
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		if (flag == unlock && unlockStatus == UnlockStatus.OnLost)
		{
			SetObjectiveStatus();
		}
	}

	private void SetObjectiveStatus()
	{
		switch (setStatus)
		{
		case ObjectiveStatus.Complete:
			GameHelper.Quests.CompleteObjective(targetObjective);
			break;
		case ObjectiveStatus.Fail:
			GameHelper.Quests.FailObjective(targetObjective);
			break;
		case ObjectiveStatus.Start:
			GameHelper.Quests.GiveObjective(targetObjective);
			break;
		}
	}

	public override string GetDescription()
	{
		string text = ((base.OwnerBlueprint as BlueprintQuestObjective == targetObjective) ? "" : $" {targetObjective}");
		string text2 = ((unlockStatus == UnlockStatus.OnGain) ? "unlocked" : "locked");
		return $"{setStatus}{text} when {unlock} is {text2}";
	}

	UnlockableFlagReferenceType IUnlockableFlagReference.GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (unlock == flag)
		{
			return UnlockableFlagReferenceType.Check;
		}
		return UnlockableFlagReferenceType.None;
	}

	QuestObjectiveReferenceType IQuestObjectiveReference.GetUsagesFor(BlueprintQuestObjective questObj)
	{
		if (questObj == targetObjective)
		{
			switch (setStatus)
			{
			case ObjectiveStatus.Start:
				return QuestObjectiveReferenceType.Give;
			case ObjectiveStatus.Complete:
				return QuestObjectiveReferenceType.Complete;
			case ObjectiveStatus.Fail:
				return QuestObjectiveReferenceType.Fail;
			}
		}
		return QuestObjectiveReferenceType.None;
	}
}
