using Code.Visual.Animation;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

public class UnitAnimationController : BaseUnitController, IControllerStart, IController
{
	public void OnStart()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			if (!allUnit.IsSleeping)
			{
				TickOnUnit(allUnit);
				AbstractUnitEntityView abstractUnitEntityView = allUnit.View.Or(null);
				if ((object)abstractUnitEntityView != null)
				{
					abstractUnitEntityView.AnimationManager.Or(null)?.CustomUpdate(allUnit.Random.Range(0.5f, 1.5f));
				}
			}
		}
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitAnimationManager unitAnimationManager = unit.View.Or(null)?.AnimationManager;
		if ((object)unitAnimationManager != null)
		{
			TickManagerOnUnit(unitAnimationManager, unit, isMechadendrite: false);
		}
		UnitPartMechadendrites optional = unit.GetOptional<UnitPartMechadendrites>();
		if (optional == null)
		{
			return;
		}
		foreach (MechadendriteSettings value in optional.Mechadendrites.Values)
		{
			UnitAnimationManager animationManager = value.AnimationManager;
			if ((object)animationManager != null)
			{
				TickManagerOnUnit(animationManager, unit, isMechadendrite: true);
			}
		}
	}

	private static void TickManagerOnUnit(UnitAnimationManager manager, AbstractUnitEntity unit, bool isMechadendrite)
	{
		using (ProfileScope.New("Tick Animator", manager))
		{
			using (ProfileScope.New("Set Variables", manager))
			{
				AbstractUnitCommand current = unit.Commands.Current;
				manager.WalkSpeedType = (unit.Features.IsCharging ? WalkSpeedType.Run : (current?.MovementType ?? manager.WalkSpeedType));
				PartUnitStealth optional = unit.GetOptional<PartUnitStealth>();
				if (manager.WalkSpeedType == WalkSpeedType.Walk && optional != null && optional.Active && !optional.FullSpeed)
				{
					manager.WalkSpeedType = WalkSpeedType.Crouch;
				}
				float currentSpeedMps = unit.Movable.CurrentSpeedMps;
				if (manager.WalkSpeedType == WalkSpeedType.Walk && currentSpeedMps < 0.95f * unit.Blueprint.Speed.Meters / 2.5f)
				{
					manager.WalkSpeedType = WalkSpeedType.Walk;
				}
				manager.IsWaitingForIncomingAttackOfOpportunity = unit.IsWaitingForIncomingAttackOfOpportunity();
				manager.Speed = 1f;
				manager.IsStopping = unit.View.MovementAgent.IsStopping;
				UnitEntityView unitEntityView = unit.View as UnitEntityView;
				manager.IsInCombat = unit.IsInCombat || (unitEntityView != null && (unitEntityView.HandsEquipment?.InCombat ?? false));
				manager.IsMechadendrite = isMechadendrite;
				bool flag = manager.IsInCombat && unitEntityView != null && unitEntityView.HandsEquipment != null;
				if (unit.GetOptional<UnitPartMechadendrites>() != null)
				{
					bool flag2 = flag && unitEntityView.HandsEquipment.Sets.Count > 0;
					if (isMechadendrite)
					{
						WeaponType activeMainHandWeaponType = ((flag2 && HasAppropriateWeapon(unitEntityView, isMainHand: true, isMelee: false)) ? unitEntityView.HandsEquipment.ActiveMainHandWeaponType : WeaponType.Fist);
						WeaponType activeOffHandWeaponType = ((flag2 && HasAppropriateWeapon(unitEntityView, isMainHand: false, isMelee: false)) ? unitEntityView.HandsEquipment.ActiveOffHandWeaponType : WeaponType.Fist);
						manager.ActiveMainHandWeaponType = activeMainHandWeaponType;
						manager.ActiveOffHandWeaponType = activeOffHandWeaponType;
					}
					else
					{
						WeaponType activeMainHandWeaponType2 = ((flag2 && HasAppropriateWeapon(unitEntityView, isMainHand: true, isMelee: true)) ? unitEntityView.HandsEquipment.ActiveMainHandWeaponType : WeaponType.Fist);
						WeaponType activeOffHandWeaponType2 = ((flag2 && HasAppropriateWeapon(unitEntityView, isMainHand: false, isMelee: true)) ? unitEntityView.HandsEquipment.ActiveOffHandWeaponType : WeaponType.Fist);
						manager.ActiveMainHandWeaponType = activeMainHandWeaponType2;
						manager.ActiveOffHandWeaponType = activeOffHandWeaponType2;
					}
				}
				else
				{
					manager.ActiveMainHandWeaponType = (flag ? unitEntityView.HandsEquipment.ActiveMainHandWeaponType : WeaponType.Fist);
					manager.ActiveOffHandWeaponType = (flag ? unitEntityView.HandsEquipment.ActiveOffHandWeaponType : WeaponType.Fist);
				}
				manager.Orientation = unit.Orientation;
				manager.DesiredOrientation = unit.DesiredOrientation;
				manager.IsDead = unit.LifeState.IsFinallyDead;
				manager.IsProne = unit.IsProne;
				manager.IsSleeping = unit.Features.Sleeping;
				manager.IsDisabled = unit.Features.CantAct;
				manager.IsAnimating = true;
				manager.IsUnconscious = unit.LifeState.IsUnconscious;
				manager.CoverType = ((manager.IsInCombat && unit is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.GetCoverType() : LosCalculations.CoverType.Obstacle);
				manager.CombatMicroIdle = (unit.Commands.Empty ? CombatMicroIdle.Weapon : CombatMicroIdle.None);
			}
			float gameDeltaTime = Game.Instance.Controllers.TimeController.GameDeltaTime;
			manager.Tick(gameDeltaTime);
		}
	}

	private static bool HasAppropriateWeapon(UnitEntityView unitView, bool isMainHand, bool isMelee)
	{
		WeaponSet selectedWeaponSet = unitView.HandsEquipment.GetSelectedWeaponSet();
		if (!isMainHand)
		{
			BlueprintItemWeapon obj = (BlueprintItemWeapon)(selectedWeaponSet.OffHand.VisibleItem?.Blueprint);
			if (obj == null)
			{
				return false;
			}
			return obj.IsMelee == isMelee;
		}
		BlueprintItemWeapon obj2 = (BlueprintItemWeapon)(selectedWeaponSet.MainHand.VisibleItem?.Blueprint);
		if (obj2 == null)
		{
			return false;
		}
		return obj2.IsMelee == isMelee;
	}

	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.IsInFogOfWar)
		{
			return false;
		}
		return true;
	}
}
