using System;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/CommandUnitAttack")]
[TypeId("962e4ff1f0264d03bba1bf1df5fc70ef")]
public class CommandUnitAttack : CommandBase
{
	private class Data
	{
		public AbstractUnitEntity Unit { get; set; }

		public MechanicEntity Target { get; set; }

		public ItemEntityWeapon Weapon { get; set; }

		public AbilityData Ability { get; set; }

		[CanBeNull]
		public UnitCommandHandle MoveCmdHandle { get; set; }

		[CanBeNull]
		public UnitCommandHandle AttackCmdHandle { get; set; }

		public bool CutsceneWeaponSet { get; set; }

		public bool IsInterrupted { get; set; }

		public IAbilityCustomAnimation CustomAnimation { get; set; }

		public Path Path { get; set; }
	}

	public enum AttackType
	{
		SingleShot,
		BurstFire,
		Melee,
		Custom
	}

	public AttackType Type;

	public bool Continuous;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ShowIf("OverrideSpeed")]
	public float Speed = 5f;

	[SerializeReference]
	public MechanicEntityEvaluator Target;

	[SerializeField]
	[ShowIf("IsCustomWeaponVisible")]
	private BlueprintItemWeaponReference m_CustomWeapon;

	public AttackHitPolicyType HitPolicy = AttackHitPolicyType.AutoHit;

	public DamagePolicyType DamagePolicy = DamagePolicyType.FxOnly;

	[Tooltip("Won't kill if Command is set to Repeat (Continuous)")]
	public bool KillTarget;

	public bool NeedLoS;

	public bool EnableLog;

	public bool MuteAttacker;

	private bool IsCustomWeaponVisible => Type == AttackType.Custom;

	public BlueprintItemWeapon CustomWeapon => m_CustomWeapon;

	[Obsolete("WH2-7940")]
	public BlueprintAbilityFXSettings.Reference SpellWeaponFXSettings => ConfigRoot.Instance.CutsceneRoot.SpellWeaponFXSettings;

	public override bool IsContinuous => Continuous;

	public override bool ShouldHaveControlledUnit => true;

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (Unit == null || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Unit.TryGetValue(out var unit))
		{
			return CommandResult.Fail("Unit not found");
		}
		if (!Target.TryGetValue(out var target))
		{
			return CommandResult.Fail("Target not found");
		}
		GetAttackData(out var weaponBlueprint, out var abilityData, out var weapon, out var cutsceneWeaponSet, out var needApproach);
		IAbilityCustomAnimation abilityCustomAnimation = weaponBlueprint.WeaponAbilities.FirstOrDefault()?.Ability?.GetComponent<IAbilityCustomAnimation>();
		Data data = player.GetCommandData<Data>(this);
		data.Unit = unit;
		data.Target = target;
		data.Ability = abilityData;
		data.Weapon = weapon;
		data.CutsceneWeaponSet = cutsceneWeaponSet;
		data.CustomAnimation = abilityCustomAnimation;
		if (Continuous && unit.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(inCombat: true);
		}
		if (skipping && KillTarget)
		{
			target.GetLifeStateOptional().MarkedForDeath = true;
			PFLog.Default.Log($"CommandUnitAttack.OnRun() skipping cutscene and killing target {target} by unit: {unit}");
		}
		if (needApproach)
		{
			float distance = weapon.AttackRange;
			data.Path = PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, target.Position, distance, 1, delegate(ForcedPath path)
			{
				data.Path = null;
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else
				{
					UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, target.Position, distance)
					{
						OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null),
						MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation)
					};
					unitMoveToParams.MarkFromCutscene();
					data.MoveCmdHandle = unit.Commands.Run(unitMoveToParams);
				}
			});
		}
		else
		{
			UnitUseAbilityParams cmdParams = CreateAttackCommandParams(abilityData, target, abilityCustomAnimation);
			data.AttackCmdHandle = unit.Commands.Run(cmdParams);
		}
		if (MuteAttacker)
		{
			AkUnitySoundEngine.SetRTPCValue("MuteEntity", 0f, Unit.GetValue().View.gameObject);
		}
		return CommandResult.Success;
	}

	private CommandResult GetAttackData(out BlueprintItemWeapon weaponBlueprint, out AbilityData abilityData, out ItemEntityWeapon weapon, out bool cutsceneWeaponSet, out bool needApproach)
	{
		weaponBlueprint = null;
		abilityData = null;
		weapon = null;
		cutsceneWeaponSet = false;
		needApproach = false;
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Unit not found");
		}
		if (!Target.TryGetValue(out var value2))
		{
			return CommandResult.Fail("Target not found");
		}
		AttackType attackType = Type;
		if (Type == AttackType.Custom && CustomWeapon != null)
		{
			weaponBlueprint = CustomWeapon;
		}
		else if (Type == AttackType.SingleShot)
		{
			(weaponBlueprint, attackType) = FindSuitableSingleShot(value);
		}
		else
		{
			weaponBlueprint = FindSuitableWeapon(value, Type, activeSet: true, notActiveSets: true);
		}
		if (weaponBlueprint == null)
		{
			if (Type == AttackType.Melee)
			{
				weaponBlueprint = ConfigRoot.Instance.CutsceneRoot.DefaultWeaponMelee;
			}
			else if (Type == AttackType.SingleShot || Type == AttackType.BurstFire)
			{
				weaponBlueprint = ConfigRoot.Instance.CutsceneRoot.DefaultWeaponRanged;
			}
		}
		if (weaponBlueprint == null)
		{
			return CommandResult.FailWithReport("Can't find suitable weapon");
		}
		BlueprintAbility abilityBlueprint = GetAbilityBlueprint(attackType, weaponBlueprint, Type);
		cutsceneWeaponSet = false;
		weapon = GetWeapon(value, weaponBlueprint, ref cutsceneWeaponSet, ref abilityBlueprint);
		abilityData = new AbilityData(abilityBlueprint, value)
		{
			OverrideWeapon = weapon,
			FXSettingsOverride = FindSuitableFXSettings(weapon, attackType)
		};
		float distanceToTarget = value.DistanceToInCells(value2);
		needApproach = CalculateNeedForApproach(distanceToTarget, weapon, abilityBlueprint);
		return CommandResult.Success;
	}

	private bool CalculateNeedForApproach(float distanceToTarget, ItemEntityWeapon weapon, BlueprintAbility abilityBlueprint)
	{
		if (CanOverrideBecauseOfStaffWeapon(weapon.Blueprint, Type) && abilityBlueprint.Range == AbilityRange.Custom)
		{
			return distanceToTarget > (float)abilityBlueprint.CustomRange;
		}
		return distanceToTarget > (float)weapon.AttackRange;
	}

	private static bool CanOverrideBecauseOfStaffWeapon(BlueprintItemWeapon weapon, AttackType type)
	{
		if (type != AttackType.Custom)
		{
			return weapon.VisualParameters.WeaponType == WeaponType.Staff;
		}
		return false;
	}

	private static BlueprintAbility GetAbilityBlueprint(AttackType actualType, BlueprintItemWeapon weaponBlueprint, AttackType type)
	{
		if (CanOverrideBecauseOfStaffWeapon(weaponBlueprint, type))
		{
			return ConfigRoot.Instance.CutsceneRoot.AttackSpell;
		}
		return actualType switch
		{
			AttackType.SingleShot => ConfigRoot.Instance.CutsceneRoot.AttackSingle, 
			AttackType.BurstFire => ConfigRoot.Instance.CutsceneRoot.AttackBurst, 
			AttackType.Melee => ConfigRoot.Instance.CutsceneRoot.AttackSingle, 
			AttackType.Custom => ConfigRoot.Instance.CutsceneRoot.AttackSingle, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private ItemEntityWeapon GetWeapon(AbstractUnitEntity unit, BlueprintItemWeapon weaponBlueprint, ref bool cutsceneWeaponSet, ref BlueprintAbility abilityBlueprint)
	{
		ItemEntityWeapon itemEntityWeapon = null;
		PartUnitBody bodyOptional = unit.GetBodyOptional();
		if (bodyOptional == null)
		{
			return null;
		}
		foreach (HandSlot item in bodyOptional.HandsEquipmentSets.SelectMany((HandsEquipmentSet set) => set.Hands))
		{
			if (item.MaybeWeapon?.Blueprint == weaponBlueprint)
			{
				itemEntityWeapon = item.Weapon;
				break;
			}
		}
		if (itemEntityWeapon == null)
		{
			itemEntityWeapon = weaponBlueprint.CreateEntity<ItemEntityWeapon>();
			bodyOptional.SetCutsceneHandsEquipment(itemEntityWeapon);
			cutsceneWeaponSet = true;
		}
		return itemEntityWeapon;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.MoveCmdHandle != null && commandData.MoveCmdHandle.Cmd != null && commandData.MoveCmdHandle.Result != AbstractUnitCommand.ResultType.Success)
		{
			commandData.Unit.Translocate(commandData.MoveCmdHandle.Cmd.Params.ForcedPath.vectorPath.LastItem(), null);
		}
		if (commandData.CutsceneWeaponSet)
		{
			commandData.Unit.GetBodyOptional()?.SetCutsceneHandsEquipment(null);
		}
		if (Continuous && commandData.Unit.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(inCombat: false);
		}
		player.ClearCommandData(this);
		if (MuteAttacker)
		{
			AkUnitySoundEngine.SetRTPCValue("MuteEntity", 1f, Unit.GetValue().View.gameObject);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		if (!Unit.TryGetValue(out var unit))
		{
			return CommandResult.Fail("Unit not found");
		}
		if (!Target.TryGetValue(out var value))
		{
			return CommandResult.Fail("Target not found");
		}
		GetAttackData(out var _, out var _, out var weapon, out var cutsceneWeaponSet, out var needApproach);
		if (cutsceneWeaponSet)
		{
			unit.GetBodyOptional()?.SetCutsceneHandsEquipment(null);
		}
		if (needApproach)
		{
			float approachRadiusMeters = weapon.AttackRange;
			PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, value.Position, approachRadiusMeters, 1, delegate(ForcedPath path)
			{
				unit.Translocate(path.vectorPath.LastItem(), null);
			});
		}
		if (!Continuous && KillTarget)
		{
			value.GetLifeStateOptional().MarkedForDeath = true;
		}
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (Continuous)
		{
			return false;
		}
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.IsInterrupted)
		{
			return true;
		}
		UnitCommandHandle moveCmdHandle = commandData.MoveCmdHandle;
		if (moveCmdHandle != null && !moveCmdHandle.IsFinished)
		{
			return false;
		}
		if (commandData.AttackCmdHandle == null)
		{
			return false;
		}
		moveCmdHandle = commandData.AttackCmdHandle;
		if (moveCmdHandle != null && !moveCmdHandle.IsFinished)
		{
			return false;
		}
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		UnitUseAbility unitUseAbility = (UnitUseAbility)(commandData.AttackCmdHandle?.Cmd);
		UnitUseAbilityParams unitUseAbilityParams = null;
		UnitCommandHandle moveCmdHandle = commandData.MoveCmdHandle;
		if (moveCmdHandle != null && moveCmdHandle.IsFinished)
		{
			commandData.MoveCmdHandle = null;
			unitUseAbilityParams = CreateAttackCommandParams(commandData.Ability, commandData.Target, commandData.CustomAnimation);
		}
		else if (IsContinuous && commandData.MoveCmdHandle == null && (unitUseAbility == null || (unitUseAbility.IsFinished && unitUseAbility.ExecutionProcess.IsEnded)))
		{
			unitUseAbilityParams = CreateAttackCommandParams(commandData.Ability, commandData.Target, commandData.CustomAnimation);
		}
		if (unitUseAbilityParams != null)
		{
			commandData.AttackCmdHandle = commandData.Unit.Commands.Run(unitUseAbilityParams);
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		UnitCommandHandle moveCmdHandle = commandData.MoveCmdHandle;
		if (moveCmdHandle != null && !moveCmdHandle.IsFinished)
		{
			moveCmdHandle.Interrupt();
		}
		UnitCommandHandle attackCmdHandle = commandData.AttackCmdHandle;
		if (attackCmdHandle != null && !attackCmdHandle.IsFinished)
		{
			attackCmdHandle.Interrupt();
		}
		if (commandData.CutsceneWeaponSet)
		{
			commandData.Unit?.GetBodyOptional()?.SetCutsceneHandsEquipment(null);
			commandData.CutsceneWeaponSet = false;
		}
		commandData.IsInterrupted = true;
		return CommandResult.Success;
	}

	private UnitUseAbilityParams CreateAttackCommandParams(AbilityData ability, MechanicEntity target, IAbilityCustomAnimation customAnimationOverride = null)
	{
		bool killTarget = !Continuous && KillTarget;
		return new UnitUseAbilityParams(ability, target)
		{
			DisableLog = !EnableLog,
			HitPolicy = HitPolicy,
			DamagePolicy = DamagePolicy,
			NeedLoS = NeedLoS,
			KillTarget = killTarget,
			CustomAnimationOverride = customAnimationOverride,
			DisableCameraFollow = true
		};
	}

	private static (BlueprintItemWeapon Weapon, AttackType Type) FindSuitableSingleShot(AbstractUnitEntity unit)
	{
		AttackType item;
		return (Weapon: FindSuitableWeapon(unit, item = AttackType.SingleShot, activeSet: true, notActiveSets: false) ?? FindSuitableWeapon(unit, item = AttackType.Melee, activeSet: true, notActiveSets: false) ?? FindSuitableWeapon(unit, item = AttackType.SingleShot, activeSet: false, notActiveSets: true) ?? FindSuitableWeapon(unit, item = AttackType.Melee, activeSet: false, notActiveSets: true) ?? FindSuitableWeapon(unit, item = AttackType.BurstFire, activeSet: true, notActiveSets: true), Type: item);
	}

	[CanBeNull]
	private static BlueprintItemWeapon FindSuitableWeapon(AbstractUnitEntity unit, AttackType type, bool activeSet, bool notActiveSets)
	{
		PartUnitBody bodyOptional = unit.GetBodyOptional();
		if (bodyOptional == null)
		{
			return null;
		}
		HandsEquipmentSet currentHandsEquipmentSet = bodyOptional.CurrentHandsEquipmentSet;
		if (activeSet)
		{
			BlueprintItemWeapon blueprintItemWeapon = GetSuitableWeaponInternal(currentHandsEquipmentSet.PrimaryHand, type) ?? GetSuitableWeaponInternal(currentHandsEquipmentSet.SecondaryHand, type);
			if (blueprintItemWeapon != null)
			{
				return blueprintItemWeapon;
			}
		}
		if (notActiveSets)
		{
			foreach (HandsEquipmentSet handsEquipmentSet in bodyOptional.HandsEquipmentSets)
			{
				if (handsEquipmentSet != currentHandsEquipmentSet)
				{
					BlueprintItemWeapon blueprintItemWeapon2 = GetSuitableWeaponInternal(handsEquipmentSet.PrimaryHand, type) ?? GetSuitableWeaponInternal(handsEquipmentSet.SecondaryHand, type);
					if (blueprintItemWeapon2 != null)
					{
						return blueprintItemWeapon2;
					}
				}
			}
		}
		return null;
		static BlueprintItemWeapon GetSuitableWeaponInternal(HandSlot slot, AttackType t)
		{
			ItemEntityWeapon maybeWeapon = slot.MaybeWeapon;
			if (maybeWeapon == null)
			{
				return null;
			}
			if (t switch
			{
				AttackType.SingleShot => maybeWeapon.Blueprint.IsRanged ? 1 : 0, 
				AttackType.BurstFire => (maybeWeapon.Blueprint.IsRanged && maybeWeapon.GetWeaponStats().ResultRateOfFire > 1) ? 1 : 0, 
				AttackType.Melee => maybeWeapon.Blueprint.IsMelee ? 1 : 0, 
				_ => 0, 
			} == 0)
			{
				return null;
			}
			return maybeWeapon.Blueprint;
		}
	}

	[CanBeNull]
	private static BlueprintAbilityFXSettings FindSuitableFXSettings(ItemEntityWeapon weapon, AttackType type)
	{
		BlueprintAbilityFXSettings blueprintAbilityFXSettings = null;
		foreach (Ability ability in weapon.Abilities)
		{
			AbilityData maybeData = ability.MaybeData;
			if (!(maybeData == null) && ability.Data.FXSettings != null)
			{
				blueprintAbilityFXSettings = ability.Data.FXSettings;
				if ((type == AttackType.SingleShot && (maybeData.IsSingleTarget || maybeData.IsAoe)) || (type == AttackType.BurstFire && maybeData.IsBurst) || (type == AttackType.Melee && maybeData.IsMelee) || type == AttackType.Custom)
				{
					return blueprintAbilityFXSettings;
				}
			}
		}
		if (blueprintAbilityFXSettings == null)
		{
			PFLog.Default.ErrorWithReport("Can't find FXSettings for cutscene attack ability");
		}
		return blueprintAbilityFXSettings;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + " <b>attacks</b> " + (Target ? Target.GetCaptionShort() : "???");
	}
}
