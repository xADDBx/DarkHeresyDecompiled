using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Covers;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EntityCoverUIState : ViewModel
{
	private readonly EntityRef<MechanicEntity> m_Entity;

	private readonly ReactiveProperty<LosCalculations.CoverType> m_CoverType;

	private TurnController TurnController => Game.Instance.Controllers.TurnController;

	private VirtualPositionController PositionController => Game.Instance.Controllers.VirtualPositionController;

	public ReadOnlyReactiveProperty<LosCalculations.CoverType> CoverType => m_CoverType;

	public EntityCoverUIState(EntityRef<MechanicEntity> entity)
	{
		m_Entity = entity;
		m_CoverType = new ReactiveProperty<LosCalculations.CoverType>().AddTo(this);
		UpdateCoverTypeInternal(TurnController.TurnBasedModeActive, null);
	}

	public void UpdateCoverType(bool? isTurnBased = null, Vector3? position = null)
	{
		bool valueOrDefault = isTurnBased.GetValueOrDefault();
		if (!isTurnBased.HasValue)
		{
			valueOrDefault = TurnController.TurnBasedModeActive;
			isTurnBased = valueOrDefault;
		}
		UpdateCoverTypeInternal(isTurnBased.Value, position);
	}

	private void UpdateCoverTypeInternal(bool isTurnBased, Vector3? shootingPosition)
	{
		if (isTurnBased)
		{
			MechanicEntity entity = m_Entity.Entity;
			if (entity != null && entity.IsPlayerEnemy && TryGetCurrentPlayerCharacter(out var currentUnit))
			{
				Vector3 valueOrDefault = shootingPosition.GetValueOrDefault();
				if (!shootingPosition.HasValue)
				{
					valueOrDefault = GetShootingPosition(currentUnit);
					shootingPosition = valueOrDefault;
				}
				if (IsMeleeAction(currentUnit))
				{
					bool flag = LosCalculations.HasMeleeLos(shootingPosition.Value, currentUnit.SizeRect, m_Entity.Entity.Position, m_Entity.Entity.SizeRect);
					m_CoverType.Value = ((!flag) ? LosCalculations.CoverType.LosBlocker : LosCalculations.CoverType.Obstacle);
				}
				else
				{
					m_CoverType.Value = LosCalculations.GetWarhammerLos(shootingPosition.Value, currentUnit.SizeRect, m_Entity);
				}
				return;
			}
		}
		m_CoverType.Value = LosCalculations.CoverType.Obstacle;
	}

	private static bool IsMeleeAction(MechanicEntity currentUnit)
	{
		AbilityData abilityData = Game.Instance.Controllers.SelectedAbilityHandler?.Ability;
		if (abilityData != null)
		{
			return abilityData.IsMelee;
		}
		return ((currentUnit as UnitEntity)?.Body.CurrentHandsEquipmentSet?.PrimaryHand.MaybeWeapon)?.Blueprint.IsMelee ?? false;
	}

	private bool TryGetCurrentPlayerCharacter(out MechanicEntity currentUnit)
	{
		currentUnit = TurnController.CurrentUnit;
		return currentUnit?.IsInPlayerParty ?? false;
	}

	private Vector3 GetShootingPosition(MechanicEntity currentUnit)
	{
		if (m_Entity.Entity == null)
		{
			return Vector3.zero;
		}
		return LosCalculations.GetBestShootingPosition(PositionController.GetDesiredPosition(currentUnit), currentUnit.SizeRect, m_Entity.Entity.Position, m_Entity.Entity.SizeRect);
	}
}
