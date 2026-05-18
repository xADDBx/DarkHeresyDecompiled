using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitPortraitPartVM : ViewModel, IEntitySubscriber, IUnitPortraitChangedHandler<EntitySubscriber>, IUnitPortraitChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitPortraitChangedHandler, EntitySubscriber>
{
	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>(null);

	private readonly ReactiveProperty<bool> m_IsDead = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCrippled = new ReactiveProperty<bool>();

	private EntityRef<BaseUnitEntity> m_Unit;

	private IDisposable m_Subscription;

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public ReadOnlyReactiveProperty<bool> IsDead => m_IsDead;

	public ReadOnlyReactiveProperty<bool> IsCrippled => m_IsCrippled;

	public IEntity GetSubscribingEntity()
	{
		return m_Unit.Entity;
	}

	public UnitPortraitPartVM()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateFields();
		}).AddTo(this);
	}

	private void UpdateFields()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess && m_Unit.Entity != null)
		{
			bool isFinallyDead = m_Unit.Entity.LifeState.IsFinallyDead;
			bool value = (bool)m_Unit.Entity.GetOptional<UnitPartDeathDoor>() && !isFinallyDead;
			m_IsDead.Value = isFinallyDead;
			m_IsCrippled.Value = value;
		}
	}

	public void SetUnitData(BaseUnitEntity unit)
	{
		m_Subscription?.Dispose();
		m_Subscription = null;
		m_Unit = unit;
		m_Portrait.Value = m_Unit.Entity?.Portrait.SmallPortrait;
		UpdateFields();
		if (m_Unit.Entity != null)
		{
			m_Subscription = EventBus.Subscribe(this);
		}
	}

	public void HandlePortraitChanged()
	{
		m_Portrait.Value = m_Unit.Entity?.Portrait.SmallPortrait;
	}

	protected override void OnDispose()
	{
		m_Subscription?.Dispose();
		m_Subscription = null;
	}
}
