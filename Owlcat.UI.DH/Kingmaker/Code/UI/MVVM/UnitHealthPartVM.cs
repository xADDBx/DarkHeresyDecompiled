using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitHealthPartVM : CharInfoHitPointsVM, IDamageHandler, ISubscriber, IHealingHandler, IActorStatChangedHandler, ISubscriber<IMechanicEntity>, ITurnStartHandler
{
	private readonly ReactiveProperty<bool> m_IsDead = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsEnemy = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPlayer = new ReactiveProperty<bool>(value: false);

	private bool m_RefreshDataRequested;

	public ReadOnlyReactiveProperty<bool> IsDead => m_IsDead;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_IsEnemy;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_IsPlayer;

	public UnitHealthPartVM(ReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		Observable.EveryUpdate().Subscribe(RefreshInternal).AddTo(this);
	}

	public UnitHealthPartVM(BaseUnitEntity unit)
		: this(new ReactiveProperty<BaseUnitEntity>(unit))
	{
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();
		BaseUnitEntity currentValue = Unit.CurrentValue;
		bool flag = currentValue != null && !currentValue.IsDisposed;
		m_IsDead.Value = flag && UnitUIWrapper.IsFinallyDead;
		m_IsPlayer.Value = flag && UnitUIWrapper.IsPlayerFaction;
		m_IsEnemy.Value = flag && UnitUIWrapper.IsPlayerEnemy;
	}

	void IDamageHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		RequestDataRefresh();
	}

	void IHealingHandler.HandleHealing(RuleHealDamage healDamage)
	{
		RequestDataRefresh();
	}

	void IActorStatChangedHandler.HandleActorStatChanged(StatChangeSet stats)
	{
		RequestDataRefresh();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateValues();
	}

	private void RequestDataRefresh()
	{
		m_RefreshDataRequested = true;
	}

	private void RefreshInternal()
	{
		if (m_RefreshDataRequested)
		{
			m_RefreshDataRequested = false;
			RefreshData();
		}
	}
}
