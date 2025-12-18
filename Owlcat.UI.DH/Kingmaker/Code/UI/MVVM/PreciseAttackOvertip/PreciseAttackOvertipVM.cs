using System;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.PreciseAttackOvertip;

public class PreciseAttackOvertipVM : ViewModel
{
	private readonly ReactiveProperty<OvertipData> m_overtipData;

	private MechanicEntity m_entity;

	private IDisposable m_AbilitySubscription;

	public ReadOnlyReactiveProperty<OvertipData> OvertipData => m_overtipData;

	public PreciseAttackOvertipVM(Observable<MechanicEntity> target)
	{
		m_overtipData = new ReactiveProperty<OvertipData>().AddTo(this);
		target.Subscribe(HandleTargetSet).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_AbilitySubscription?.Dispose();
		m_entity = null;
	}

	private void HandleTargetSet(MechanicEntity entity)
	{
		m_AbilitySubscription?.Dispose();
		m_entity = entity;
		if (entity != null)
		{
			MechanicEntityUIState orCreateUnitState = UnitUIStateHolder.Instance.GetOrCreateUnitState(entity);
			m_AbilitySubscription = orCreateUnitState.AbilityTargetUIData.Subscribe(HandleAbilityChanged);
		}
	}

	private void HandleAbilityChanged(AbilityTargetUIData abilityData)
	{
		if (abilityData.Ability == null || !abilityData.Ability.IsPrecise)
		{
			m_overtipData.Value = default(OvertipData);
			return;
		}
		PartHealth healthOptional = m_entity.GetHealthOptional();
		PartArmor armorOptional = m_entity.GetArmorOptional();
		ModifiableValue modifiableValue = armorOptional?.Durability;
		int num = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
		UIDamagePredictionData damage = abilityData.Damage;
		ReactiveProperty<OvertipData> overtipData = m_overtipData;
		OvertipData value = new OvertipData
		{
			HealthLeft = (healthOptional?.HitPointsLeft ?? 0),
			ArmorLeft = (armorOptional?.DurabilityLeft ?? 0)
		};
		ModifiableValueHitPoints modifiableValueHitPoints = healthOptional?.HitPoints;
		value.HealthMax = ((modifiableValueHitPoints != null) ? ((int)modifiableValueHitPoints) : 0);
		value.ArmorMax = num;
		value.HealthDamage = damage.HealthMaxDamage;
		value.ArmorDamage = damage.ArmorMaxDamage;
		value.HasArmor = num > 0;
		value.Faction = (m_entity.IsInPlayerParty ? Faction.Player : (m_entity.IsPlayerEnemy ? Faction.Enemy : Faction.None));
		overtipData.Value = value;
	}
}
