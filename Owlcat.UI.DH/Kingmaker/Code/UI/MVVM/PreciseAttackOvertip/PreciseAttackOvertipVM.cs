using System;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.PreciseAttackOvertip;

public class PreciseAttackOvertipVM : ViewModel
{
	private readonly ReactiveProperty<OvertipData> m_OvertipData;

	private MechanicEntityUIState m_EntityUIState;

	private IDisposable m_AbilitySubscription;

	public ReadOnlyReactiveProperty<OvertipData> OvertipData => m_OvertipData;

	public bool IsCountHpAsArmor => m_EntityUIState.IsCountHpAsArmor;

	public PreciseAttackOvertipVM(Observable<MechanicEntity> target)
	{
		m_OvertipData = new ReactiveProperty<OvertipData>().AddTo(this);
		target.Subscribe(HandleTargetSet).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_AbilitySubscription?.Dispose();
		m_EntityUIState = null;
	}

	private void HandleTargetSet(MechanicEntity entity)
	{
		m_AbilitySubscription?.Dispose();
		if (entity != null)
		{
			m_EntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(entity);
			m_AbilitySubscription = m_EntityUIState.AbilityTargetUIData.Subscribe(HandleAbilityChanged);
		}
	}

	private void HandleAbilityChanged(AbilityTargetUIData abilityData)
	{
		if (abilityData.Ability == null || !abilityData.Ability.IsPrecise)
		{
			m_OvertipData.Value = default(OvertipData);
			return;
		}
		PartHealth health = m_EntityUIState.MechanicEntity.Health;
		PartArmor armor = m_EntityUIState.MechanicEntity.Armor;
		int num = armor?.DurabilityValue ?? 0;
		UIDamagePredictionData damage = abilityData.Damage;
		m_OvertipData.Value = new OvertipData
		{
			HealthLeft = (health?.HitPointsLeft ?? 0),
			ArmorLeft = (armor?.DurabilityLeft ?? 0),
			HealthMax = (health?.MaxHitPoints ?? 0),
			ArmorMax = num,
			HealthDamage = damage.HealthMaxDamage,
			ArmorDamage = damage.ArmorMaxDamage,
			HasArmor = (num > 0),
			Faction = (m_EntityUIState.MechanicEntity.MechanicEntity.IsInPlayerParty ? Faction.Player : (m_EntityUIState.MechanicEntity.IsPlayerEnemy ? Faction.Enemy : Faction.None))
		};
	}
}
