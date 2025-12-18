using System;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Gameplay.Features.Concentration;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public readonly struct MechanicEntityUIWrapper
{
	private readonly EntityRef<MechanicEntity> m_MechanicEntity;

	public MechanicEntity MechanicEntity => m_MechanicEntity.Entity;

	public string Name => m_MechanicEntity.Entity?.Name ?? "-";

	public string UniqueId => m_MechanicEntity.Id;

	public int Difficulty => UIUtilityUnit.GetSurfaceEnemyDifficulty(MechanicEntity as BaseUnitEntity);

	[CanBeNull]
	public Sprite MiddlePortrait => UIUtilityUnit.GetSurfaceCombatStandardPortrait(MechanicEntity, PortraitCombatSize.Middle);

	[CanBeNull]
	public Sprite SmallPortrait => UIUtilityUnit.GetSurfaceCombatStandardPortrait(MechanicEntity, PortraitCombatSize.Small);

	[CanBeNull]
	public Sprite Icon => UIUtilityUnit.GetSurfaceCombatStandardPortrait(MechanicEntity, PortraitCombatSize.Icon);

	[CanBeNull]
	public PartLifeState LifeState => m_MechanicEntity.Entity?.GetLifeStateOptional();

	[CanBeNull]
	public PartHealth Health
	{
		get
		{
			object obj = m_MechanicEntity.Entity?.GetLifeStateOptional()?.Health;
			if (obj == null)
			{
				MechanicEntity entity = m_MechanicEntity.Entity;
				if (entity == null)
				{
					return null;
				}
				PartDestructionStagesManager destructionStagesManagerOptional = entity.GetDestructionStagesManagerOptional();
				if (destructionStagesManagerOptional == null)
				{
					return null;
				}
				obj = destructionStagesManagerOptional.Health;
			}
			return (PartHealth)obj;
		}
	}

	[CanBeNull]
	public PartArmor Armor => m_MechanicEntity.Entity?.GetLifeStateOptional()?.Armor;

	public bool IsFinallyDead => m_MechanicEntity.Entity?.GetLifeStateOptional()?.IsFinallyDead ?? IsDead;

	public bool IsDead
	{
		get
		{
			if (m_MechanicEntity.Entity?.GetLifeStateOptional() != null)
			{
				MechanicEntity entity = m_MechanicEntity.Entity;
				if (entity != null && entity.IsDead)
				{
					return true;
				}
			}
			MechanicEntity entity2 = m_MechanicEntity.Entity;
			if (entity2 == null)
			{
				return false;
			}
			return entity2.GetDestructionStagesManagerOptional()?.Stage == DestructionStage.Destroyed;
		}
	}

	public bool IsDeadOrUnconscious
	{
		get
		{
			if (m_MechanicEntity.Entity?.GetLifeStateOptional() != null)
			{
				MechanicEntity entity = m_MechanicEntity.Entity;
				if (entity != null && entity.IsDeadOrUnconscious)
				{
					return true;
				}
			}
			MechanicEntity entity2 = m_MechanicEntity.Entity;
			if (entity2 == null)
			{
				return false;
			}
			return entity2.GetDestructionStagesManagerOptional()?.Stage == DestructionStage.Destroyed;
		}
	}

	public bool IsPlayer => (m_MechanicEntity.Entity?.GetFactionOptional()?.IsPlayer).GetValueOrDefault();

	public bool IsPlayerFaction => m_MechanicEntity.Entity?.IsPlayerFaction ?? false;

	public bool IsPlayerEnemy => m_MechanicEntity.Entity?.IsPlayerEnemy ?? false;

	public bool IsNeutral => m_MechanicEntity.Entity?.IsNeutral ?? false;

	public bool IsSquadLeader => m_MechanicEntity.Entity?.IsSquadLeader ?? false;

	public bool IsInSquad => m_MechanicEntity.Entity?.IsInSquad ?? false;

	public bool IsCover
	{
		get
		{
			MechanicEntity entity = m_MechanicEntity.Entity;
			return entity is CoverEntity || entity is ThinCoverEntity;
		}
	}

	public bool IsDestructible => m_MechanicEntity.Entity is DestructibleEntity;

	public bool IsDestructibleNotCover
	{
		get
		{
			if (IsDestructible)
			{
				return !IsCover;
			}
			return false;
		}
	}

	[CanBeNull]
	public PartUnitCombatState CombatState => m_MechanicEntity.Entity?.GetCombatStateOptional();

	public bool IsInCombat => m_MechanicEntity.Entity?.IsInCombat ?? false;

	public bool IsDisposed => m_MechanicEntity.Entity?.IsDisposed ?? false;

	public bool IsVisibleForPlayer => m_MechanicEntity.Entity?.IsVisibleForPlayer ?? false;

	public bool IsInCameraFrustum => m_MechanicEntity.Entity?.IsInCameraFrustum ?? false;

	public bool IsDirectlyControllable => m_MechanicEntity.Entity?.IsDirectlyControllable ?? false;

	public PartAdditionalCombatObjectiveUnit AdditionalCombatObjective => m_MechanicEntity.Entity?.GetOptional<PartAdditionalCombatObjectiveUnit>();

	[NotNull]
	public Initiative Initiative => m_MechanicEntity.Entity?.Initiative ?? throw new NullReferenceException("MechanicEntityUIWrapper.Initiative can't be null!");

	[CanBeNull]
	public PartStatsAttributes Attributes => m_MechanicEntity.Entity?.GetAttributesOptional();

	[CanBeNull]
	public PartMechanicFeatures Features => m_MechanicEntity.Entity?.Features;

	[CanBeNull]
	public BuffCollection Buffs => m_MechanicEntity.Entity?.Buffs;

	[CanBeNull]
	public PartStatsContainer Stats => m_MechanicEntity.Entity?.GetStatsContainerOptional();

	public bool IsDeadAndHasAttachedDroppedLoot
	{
		get
		{
			MechanicEntity entity = m_MechanicEntity.Entity;
			if (entity != null && entity.IsDead)
			{
				return m_MechanicEntity.Entity.GetOptional<PartInventory>()?.IsLootDroppedAsAttached ?? false;
			}
			return false;
		}
	}

	public bool IsDeadAndHasLoot
	{
		get
		{
			MechanicEntity entity = m_MechanicEntity.Entity;
			if (entity != null && entity.IsDead)
			{
				return m_MechanicEntity.Entity.GetOptional<PartInventory>()?.HasLoot ?? false;
			}
			return false;
		}
	}

	public Buff ConcentrationBuff => m_MechanicEntity.Entity?.Parts.GetOptional<PartConcentration>()?.Buff;

	public IUIChanneling Channeling => m_MechanicEntity.Entity?.Parts.GetOptional<PartChanneling>();

	public IUIUnitMoraleData Morale => m_MechanicEntity.Entity?.Parts.GetOptional<PartMorale>();

	public bool IsEnemy(MechanicEntity entity)
	{
		return m_MechanicEntity.Entity?.IsEnemy(entity) ?? false;
	}

	public MechanicEntityUIWrapper([NotNull] MechanicEntity mechanicEntity)
	{
		m_MechanicEntity = mechanicEntity;
	}
}
