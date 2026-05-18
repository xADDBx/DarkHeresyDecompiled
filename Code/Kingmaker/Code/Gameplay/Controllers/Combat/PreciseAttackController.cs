using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound;
using Kingmaker.UI.AR;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.Combat;

public class PreciseAttackController : IControllerTick, IController
{
	private enum SoundPrecisionType
	{
		PrecisionShotOn,
		PrecisionShotOff,
		PrecisionShotTargetChange
	}

	public class BodyPartUIData
	{
		public readonly BlueprintBodyPart BodyPart;

		public readonly bool IsHiddenByCover;

		public readonly bool RestrictionPassed;

		public BodyPartUIData(BlueprintBodyPart bodyPart, bool restrictionPassed, bool isHiddenByCover)
		{
			BodyPart = bodyPart;
			RestrictionPassed = restrictionPassed;
			IsHiddenByCover = isHiddenByCover;
		}
	}

	public struct TargetData
	{
		public readonly MechanicEntity Target;

		public readonly BlueprintBodyPart BodyPart;

		public TargetData(MechanicEntity target, BlueprintBodyPart bodyPart)
		{
			Target = target;
			BodyPart = bodyPart;
		}
	}

	private AbilityExecutionContext _context;

	private AbilityData _ability;

	private readonly ReactiveCommand<TargetData> _process = new ReactiveCommand<TargetData>();

	private readonly List<BodyPartUIData> _points = new List<BodyPartUIData>();

	private readonly ReactiveProperty<MechanicEntity> m_Target = new ReactiveProperty<MechanicEntity>();

	private readonly ReactiveCommand<bool> m_Show = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<BodyPartUIData> m_SelectedBodyPart = new ReactiveProperty<BodyPartUIData>();

	private readonly ReactiveProperty<BodyPartUIData> m_HoveredBodyPart = new ReactiveProperty<BodyPartUIData>();

	private float _playerScrollPosition;

	private float _zoomTime;

	public ReadOnlyReactiveProperty<MechanicEntity> Target => m_Target;

	public Observable<bool> Show => m_Show;

	public ReadOnlyReactiveProperty<BodyPartUIData> SelectedBodyPart => m_SelectedBodyPart;

	public ReadOnlyReactiveProperty<BodyPartUIData> HoveredBodyPart => m_HoveredBodyPart;

	public IEnumerable<BodyPartUIData> Points => _points;

	private CameraRig CameraRig => CameraRig.Instance;

	private IEnumerable<MechanicEntity> Enemies => Game.Instance.Controllers.TurnController.UnitsInCombat.Where((MechanicEntity u) => u.IsPlayerEnemy);

	public bool HasTarget => Target.CurrentValue != null;

	public bool IsTargetCovered()
	{
		return LosCalculations.GetWarhammerLos(_context.Caster, Target.CurrentValue).CoverType == LosCalculations.CoverType.Cover;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
	}

	public void SelectNextTarget()
	{
		if (_ability == null)
		{
			return;
		}
		int num = Enemies.IndexOf(_context.ClickedTarget.Entity);
		if (num == -1)
		{
			return;
		}
		int num2 = Enemies.Count();
		if (num2 < 2)
		{
			return;
		}
		int num3 = num + 1;
		MechanicEntity mechanicEntity = null;
		while (true)
		{
			if (num3 >= num2)
			{
				num3 = 0;
			}
			mechanicEntity = Enemies.ElementAtOrDefault(num3);
			if (mechanicEntity != null && _ability.CanTargetFromDesiredPosition(mechanicEntity))
			{
				break;
			}
			num3++;
			if (num3 == num)
			{
				return;
			}
		}
		_context?.Dispose();
		_context = _ability.ClaimExecutionContext(mechanicEntity);
		ClearBodyPartData();
		m_Target.Value = mechanicEntity;
		UpdatePoints();
		MoveCameraToTarget();
		PlayPrecisionSound(SoundPrecisionType.PrecisionShotTargetChange);
		TargetWrapper targetWrapper = new TargetWrapper(mechanicEntity);
		EventBus.RaiseEvent(delegate(IAbilityRangeManualUIHandler h)
		{
			h.HandleSetRangeToCasterPositionManual(_ability, targetWrapper);
		});
	}

	public void SelectPrevTarget()
	{
		if (_ability == null)
		{
			return;
		}
		int num = Enemies.IndexOf(_context.ClickedTarget.Entity);
		if (num == -1)
		{
			return;
		}
		int num2 = Enemies.Count();
		if (num2 < 2)
		{
			return;
		}
		int num3 = num - 1;
		MechanicEntity mechanicEntity;
		while (true)
		{
			if (num3 < 0)
			{
				num3 = num2 - 1;
			}
			mechanicEntity = Enemies.ElementAtOrDefault(num3);
			if (mechanicEntity != null && _ability.CanTargetFromDesiredPosition(mechanicEntity))
			{
				break;
			}
			num3--;
			if (num3 == num)
			{
				return;
			}
		}
		_context?.Dispose();
		_context = _ability.ClaimExecutionContext(mechanicEntity);
		ClearBodyPartData();
		m_Target.Value = mechanicEntity;
		UpdatePoints();
		MoveCameraToTarget();
		PlayPrecisionSound(SoundPrecisionType.PrecisionShotTargetChange);
		TargetWrapper targetWrapper = new TargetWrapper(mechanicEntity);
		EventBus.RaiseEvent(delegate(IAbilityRangeManualUIHandler h)
		{
			h.HandleSetRangeToCasterPositionManual(_ability, targetWrapper);
		});
	}

	public void SelectTargetManual(MechanicEntity target, out string unavailabilityReasonString)
	{
		unavailabilityReasonString = LocalizedTexts.Instance.Reasons.UnavailableGeneric;
		if (_ability == null || target == null)
		{
			return;
		}
		if (!_ability.CanTargetFromDesiredPosition(target, out var unavailabilityReason))
		{
			if (unavailabilityReason.HasValue)
			{
				GridNodeBase bestShootingPositionForDesiredPosition = _ability.GetBestShootingPositionForDesiredPosition(target);
				unavailabilityReasonString = _ability.GetUnavailabilityReasonString(unavailabilityReason.Value, bestShootingPositionForDesiredPosition.Vector3Position(), target);
			}
			return;
		}
		unavailabilityReasonString = null;
		_context?.Dispose();
		_context = _ability.ClaimExecutionContext(target);
		ClearBodyPartData();
		m_Target.Value = target;
		UpdatePoints();
		MoveCameraToTarget();
		PlayPrecisionSound(SoundPrecisionType.PrecisionShotTargetChange);
		TargetWrapper targetWrapper = new TargetWrapper(target);
		EventBus.RaiseEvent(delegate(IAbilityRangeManualUIHandler h)
		{
			h.HandleSetRangeToCasterPositionManual(_ability, targetWrapper);
		});
	}

	public void SetSelectedBodyPart(BodyPartUIData bodyPart)
	{
		m_SelectedBodyPart.Value = bodyPart;
	}

	public void SetHoveredBodyPart(BodyPartUIData bodyPart)
	{
		m_HoveredBodyPart.Value = bodyPart;
	}

	public float GetBodyPartHitChance(BlueprintBodyPart bodyPart)
	{
		if (_context?.Ability == null)
		{
			return 0f;
		}
		return AbilityTargetUIDataCache.Instance.GetOrCreate(_context.Ability, Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(_context.Caster), _context.ClickedTarget.Entity, bodyPart, null, null).HitChance.HitWithAvoidanceChance;
	}

	public float GetRawCriticalEffectChance(BlueprintBodyPart bodyPart, int index)
	{
		MechanicEntity entity = _context.ClickedTarget.Entity;
		if (entity == null)
		{
			return 0f;
		}
		float num = (int)entity.Actor.GetStat(StatType.SkillResistance, null, default(StatContext), "GetRawCriticalEffectChance");
		int? num2 = entity.GetLifeStateOptional()?.Health.GetCriticalStage(bodyPart) - (index + 1);
		if (num2.HasValue)
		{
			int valueOrDefault = num2.GetValueOrDefault();
			if (valueOrDefault >= 0)
			{
				return -1f;
			}
			switch (valueOrDefault)
			{
			case -1:
				return 100f - num;
			case -2:
				return (100f - num) / 100f * (100f - num);
			case -3:
				return IsMeleeWeaponAbility() ? 0f : ((100f - num) / 100f * (100f - num) / 100f * (100f - num));
			}
		}
		return 0f;
	}

	public bool IsMeleeWeaponAbility()
	{
		if (_context.Ability.Weapon != null)
		{
			return _context.Ability.Weapon.Blueprint.IsMelee;
		}
		return false;
	}

	public Observable<TargetData> RequestBodyPartOrDefault(AbilityData ability, TargetWrapper target)
	{
		if (target == null)
		{
			return Observable.Empty<TargetData>();
		}
		if (CameraRig == null || Game.Instance?.CurrentlyLoadedArea == null)
		{
			return Observable.Empty<TargetData>();
		}
		_ability = ability;
		_context = ability.ClaimExecutionContext(target);
		m_Target.Value = target.Entity;
		UpdatePoints();
		MoveCameraToTarget();
		m_Show.Execute(parameter: true);
		PlayPrecisionSound(SoundPrecisionType.PrecisionShotOn);
		return _process;
	}

	public void HandlePreciseAttackCancelled()
	{
		if (m_Target.Value != null)
		{
			_process.Execute(default(TargetData));
			HandlePreciseAttackFinished(isInterrupted: true);
		}
	}

	public void HandlePreciseAttackAccepted()
	{
		_process.Execute(new TargetData(m_Target.Value, m_SelectedBodyPart.Value?.BodyPart));
		HandlePreciseAttackFinished(isInterrupted: false);
	}

	private void HandlePreciseAttackFinished(bool isInterrupted)
	{
		m_Show.Execute(parameter: false);
		PlayPrecisionSound(SoundPrecisionType.PrecisionShotOff, isInterrupted);
		Game.Instance.CursorController.ClearCursor();
		CameraRig.UnLockCamera();
		CameraRig.CameraZoom.ZoomToTimed(_playerScrollPosition, _zoomTime);
		Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
		ClearFields();
	}

	private void PlayPrecisionSound(SoundPrecisionType type, bool isInterrupted = false)
	{
		switch (type)
		{
		case SoundPrecisionType.PrecisionShotOn:
		{
			BlueprintAbilityFXSettings fXSettings = _ability.FXSettings;
			AkSwitchReference akSwitchReference = fXSettings?.ProjectileTypeSwitch;
			if (akSwitchReference.IsValid())
			{
				AkUnitySoundEngine.SetSwitch(akSwitchReference.Group, akSwitchReference.Value, UIDollRooms.Instance.gameObject);
			}
			if (fXSettings != null && fXSettings.OverrideDamageType)
			{
				AkSwitchReference damageSoundSwitch = ConfigRoot.Instance.HitSystemRoot.GetDamageSoundSwitch(fXSettings.DamageType);
				if (damageSoundSwitch.IsValid())
				{
					AkUnitySoundEngine.SetSwitch(damageSoundSwitch.Group, damageSoundSwitch.Value, UIDollRooms.Instance.gameObject);
				}
			}
			AkUnitySoundEngine.SetState("PrecisionShotOutState", "None");
			AkUnitySoundEngine.SetState("PreciseShot", "On");
			CombatSounds.Instance.Combat.PrecisionShotOn.Play();
			break;
		}
		case SoundPrecisionType.PrecisionShotOff:
			AkUnitySoundEngine.SetState(isInterrupted ? "PrecisionShotOutState" : "PreciseShot", isInterrupted ? "Interrupted" : "None");
			CombatSounds.Instance.Combat.PrecisionShotOff.Play();
			break;
		case SoundPrecisionType.PrecisionShotTargetChange:
			CombatSounds.Instance.Combat.PrecisionShotTargetChange.Play();
			break;
		}
	}

	private void ClearFields()
	{
		_points.Clear();
		m_Target.Value = null;
		_context = null;
		_ability = null;
		ClearBodyPartData();
	}

	private void ClearBodyPartData()
	{
		m_SelectedBodyPart.Value = null;
		m_HoveredBodyPart.Value = null;
	}

	private void MoveCameraToTarget()
	{
		PreciseAttackRoot.PreciseAttackCameraOffsetEntry preciseAttackCameraOffsetEntry = ConfigRoot.Instance?.PreciseAttack?.GetCameraOffsetBySize(m_Target.Value.Size);
		if (preciseAttackCameraOffsetEntry == null)
		{
			CameraRig.ScrollTo(m_Target.Value.Position);
			CameraRig.LockCamera();
			PFLog.UI.Error("PreciseAttackRoot.GetCameraOffsetBySize doesn't have config for size: {0}", m_Target.Value.Size);
		}
		else
		{
			_playerScrollPosition = CameraRig.CameraZoom.PlayerScrollPosition;
			_zoomTime = preciseAttackCameraOffsetEntry.ZoomTime;
			Vector3 position = m_Target.Value.Position + Vector3.up * preciseAttackCameraOffsetEntry.HeightOffset;
			CameraRig.ScrollTo(position);
			CameraRig.CameraZoom.ZoomToTimed(preciseAttackCameraOffsetEntry.Zoom, preciseAttackCameraOffsetEntry.ZoomTime);
			CameraRig.LockCamera();
		}
	}

	private void UpdatePoints()
	{
		_points.Clear();
		_points.AddRange(m_Target.Value.BodyParts.Select(delegate(BlueprintBodyPart p)
		{
			bool restrictionPassed = p.Restrictions.IsPassed(EvalContext.Current);
			bool isHiddenByCover = _context.LosToClickedTarget.CoverType != 0 && p.ReplaceableByCover;
			return new BodyPartUIData(p, restrictionPassed, isHiddenByCover);
		}));
	}
}
