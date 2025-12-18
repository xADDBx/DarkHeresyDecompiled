using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/SummonPoolTrigger")]
[AllowMultipleComponents]
[TypeId("5ce1080e9c809614daae11db4baa37a4")]
public class SummonPoolTrigger : EntityFactComponentDelegate, ISummonPoolHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public enum ChangeTypes
	{
		Both,
		Ascending,
		Descending
	}

	[FormerlySerializedAs("count")]
	public int Count;

	public ChangeTypes ChangeType;

	[FormerlySerializedAs("summonPool")]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	[SerializeField]
	private BpRef<BlueprintFaction>[] m_CountWithFactions;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public void HandleUnitAdded(ISummonPool pool)
	{
		if (ChangeType == ChangeTypes.Ascending || ChangeType == ChangeTypes.Both)
		{
			OnPoolChanged(pool);
		}
	}

	public void HandleUnitRemoved(ISummonPool pool)
	{
		if (ChangeType == ChangeTypes.Descending || ChangeType == ChangeTypes.Both)
		{
			OnPoolChanged(pool);
		}
	}

	public void HandleLastUnitRemoved(ISummonPool pool)
	{
	}

	private void OnPoolChanged(ISummonPool pool)
	{
		if (SummonPool != pool.Blueprint)
		{
			return;
		}
		if (m_CountWithFactions == null || m_CountWithFactions.Length == 0)
		{
			if (pool.Count != Count)
			{
				return;
			}
		}
		else if (pool.CountWithFactions(m_CountWithFactions.Dereference()) != Count)
		{
			return;
		}
		if (Conditions.Check())
		{
			Actions.Run();
		}
	}
}
