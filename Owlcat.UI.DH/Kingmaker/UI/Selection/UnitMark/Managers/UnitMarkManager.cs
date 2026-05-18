using System.Collections.Generic;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark.Managers;

public class UnitMarkManager : MonoBehaviour, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IUnitFactionHandler, ISubscriber<IBaseUnitEntity>, IUnitChangeAttackFactionsHandler, IViewDetachedHandler, ISubscriber<IEntity>, IViewAttachedHandler
{
	public BaseUnitMark UnitMarkPrefab;

	private readonly Dictionary<AbstractUnitEntity, BaseUnitMark> m_Marks = new Dictionary<AbstractUnitEntity, BaseUnitMark>();

	public void OnEnable()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			AbstractUnitEntityView abstractUnitEntityView = allUnit.View.AsAbstractUnitEntityView();
			if ((bool)abstractUnitEntityView && UnitNeedsMark(allUnit))
			{
				AddMark(allUnit, abstractUnitEntityView);
			}
		}
		EventBus.Subscribe(this);
	}

	public void OnDisable()
	{
		EventBus.Unsubscribe(this);
		foreach (var (_, baseUnitMark2) in m_Marks)
		{
			if ((bool)baseUnitMark2)
			{
				baseUnitMark2.Dispose();
				Utils.EditorSafeDestroy(baseUnitMark2.gameObject);
			}
		}
		m_Marks.Clear();
	}

	protected virtual bool UnitNeedsMark(AbstractUnitEntity unit)
	{
		return !unit.LifeState.IsDead;
	}

	private void AddMark(AbstractUnitEntity unit, AbstractUnitEntityView view)
	{
		if (!m_Marks.ContainsKey(unit))
		{
			BaseUnitMark baseUnitMark = Object.Instantiate(UnitMarkPrefab, unit.View.ViewTransform);
			baseUnitMark.transform.localPosition = Vector3.zero;
			baseUnitMark.transform.localRotation = Quaternion.identity;
			baseUnitMark.Initialize(unit);
			m_Marks.Add(unit, baseUnitMark);
		}
	}

	private void RemoveMark(AbstractUnitEntity unit)
	{
		if (m_Marks.TryGetValue(unit, out var value))
		{
			value.Dispose();
			Utils.EditorSafeDestroy(value.gameObject);
			m_Marks.Remove(unit);
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		HandleUpdateUnitState();
	}

	public void HandleFactionChanged()
	{
		HandleUpdateUnitState();
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		HandleUpdateUnitState();
	}

	private void HandleUpdateUnitState()
	{
		AbstractUnitEntity unit = EventInvokerExtensions.AbstractUnitEntity;
		DelayedInvoker.InvokeInFrames(delegate
		{
			UpdateProperties(unit);
		}, 1);
	}

	private void UpdateProperties(AbstractUnitEntity unit)
	{
		if (UnitNeedsMark(unit))
		{
			AbstractUnitEntityView abstractUnitEntityView = unit.View.AsAbstractUnitEntityView();
			if ((object)abstractUnitEntityView != null)
			{
				AddMark(unit, abstractUnitEntityView);
				return;
			}
		}
		RemoveMark(unit);
	}

	public void OnViewDetached(IEntityView view)
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity unit)
		{
			RemoveMark(unit);
		}
	}

	public void OnViewAttached(IEntityView viewBase)
	{
		if (EventInvokerExtensions.Entity is AbstractUnitEntity unit && viewBase is AbstractUnitEntityView view && UnitNeedsMark(unit))
		{
			AddMark(unit, view);
		}
	}
}
