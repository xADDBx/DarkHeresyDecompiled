using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitHealthPartVM : CharInfoHitPointsVM
{
	private readonly ReactiveProperty<bool> m_IsDead = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsEnemy = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPlayer = new ReactiveProperty<bool>(value: false);

	public ReadOnlyReactiveProperty<bool> IsDead => m_IsDead;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_IsEnemy;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_IsPlayer;

	public UnitHealthPartVM(ReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
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
}
