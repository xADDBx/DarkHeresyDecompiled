using System;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Quests.Common;

[Obsolete]
[ComponentName("Common/SummonPoolCountTrigger")]
[TypeId("225c8b2ac0aa5d74797a17ccd71b9f6e")]
public class SummonPoolCountTrigger : QuestObjectiveComponentDelegate, ISummonPoolHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public enum ObjectiveStatus
	{
		Complete,
		Fail
	}

	public int count;

	public ObjectiveStatus setStatus;

	[SerializeField]
	[FormerlySerializedAs("summonPool")]
	private BlueprintSummonPoolReference m_summonPool;

	public BlueprintSummonPool summonPool => m_summonPool?.Get();

	public void HandleUnitAdded(ISummonPool pool)
	{
		OnPoolChanged(pool);
	}

	public void HandleUnitRemoved(ISummonPool pool)
	{
		OnPoolChanged(pool);
	}

	public void HandleLastUnitRemoved(ISummonPool pool)
	{
	}

	private void OnPoolChanged(ISummonPool pool)
	{
		if (summonPool == pool.Blueprint && pool.Count == count)
		{
			ChangeObjectiveStatus();
		}
	}

	private void ChangeObjectiveStatus()
	{
		switch (setStatus)
		{
		case ObjectiveStatus.Complete:
			base.Objective.Complete();
			break;
		case ObjectiveStatus.Fail:
			base.Objective.Fail();
			break;
		}
	}

	public override string GetDescription()
	{
		return $"{setStatus} when {summonPool} count is {count}";
	}
}
