using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.Combat;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitCombatState : BaseUnitPart, IRoundStartHandler, ISubscriber, IWarhammerAttackHandler, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IEventTag<IUnitCommandEndHandler, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IHashable, IOwlPackable<PartUnitCombatState>
{
	public interface IOwner : IEntityPartOwner<PartUnitCombatState>, IEntityPartOwner
	{
		PartUnitCombatState CombatState { get; }
	}

	[JsonProperty]
	[OwlPackInclude]
	private bool m_InCombat;

	[JsonProperty]
	[OwlPackInclude]
	private float? m_OverrideInitiative;

	[JsonProperty]
	[OwlPackInclude]
	public int DamageReceivedInCurrentFight;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_LastTarget;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntityWeapon> m_LastAbilityWeapon;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitCombatState",
		OldNames = null,
		Fields = new FieldInfo[28]
		{
			new FieldInfo("m_InCombat", typeof(bool)),
			new FieldInfo("ReturnPosition", typeof(Vector3?)),
			new FieldInfo("ReturnOrientation", typeof(float?)),
			new FieldInfo("AttackInRoundCount", typeof(int)),
			new FieldInfo("AttackedInRoundCount", typeof(int)),
			new FieldInfo("HitInRoundCount", typeof(int)),
			new FieldInfo("GotHitInRoundCount", typeof(int)),
			new FieldInfo("IsMovedThisTurn", typeof(bool)),
			new FieldInfo("SaveMPAfterUsingNextAbility", typeof(bool)),
			new FieldInfo("Surprised", typeof(bool)),
			new FieldInfo("MovementPoints", typeof(float), new string[1] { "ActionPointsBlue" }),
			new FieldInfo("MovedCellsThisTurn", typeof(float)),
			new FieldInfo("MovementPointsSpentThisTurn", typeof(float), new string[1] { "ActionPointsBlueSpentThisTurn" }),
			new FieldInfo("ActionPoints", typeof(int), new string[1] { "ActionPointsYellow" }),
			new FieldInfo("AttacksOfOpportunityMadeThisTurnCount", typeof(int)),
			new FieldInfo("ForceMovedDistanceInCells", typeof(int)),
			new FieldInfo("LastStraightMoveLength", typeof(int)),
			new FieldInfo("LastDiagonalCount", typeof(int)),
			new FieldInfo("m_OverrideInitiative", typeof(float?)),
			new FieldInfo("DamageReceivedInCurrentFight", typeof(int)),
			new FieldInfo("ManualTarget", typeof(MechanicEntity)),
			new FieldInfo("StartedCombatNearEnemy", typeof(bool)),
			new FieldInfo("m_LastTarget", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_LastAbilityWeapon", typeof(EntityRef<ItemEntityWeapon>)),
			new FieldInfo("LastAttackPosition", typeof(Vector3?)),
			new FieldInfo("TurnStartPosition", typeof(Vector3?)),
			new FieldInfo("ActionPointsAttackCount", typeof(float)),
			new FieldInfo("ActionPointsAttackCountMax", typeof(float))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public Vector3? ReturnPosition { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float? ReturnOrientation { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int AttackInRoundCount { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int AttackedInRoundCount { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int HitInRoundCount { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int GotHitInRoundCount { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsMovedThisTurn { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool SaveMPAfterUsingNextAbility { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool Surprised { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	[OwlPackOldName("ActionPointsBlue")]
	public float MovementPoints { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public float MovedCellsThisTurn { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	[OwlPackOldName("ActionPointsBlueSpentThisTurn")]
	public float MovementPointsSpentThisTurn { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	[OwlPackOldName("ActionPointsYellow")]
	public int ActionPoints { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int AttacksOfOpportunityMadeThisTurnCount { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int ForceMovedDistanceInCells { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	[Obsolete]
	public int LastStraightMoveLength { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	[Obsolete]
	public int LastDiagonalCount { get; set; }

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public MechanicEntity ManualTarget { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool StartedCombatNearEnemy { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public Vector3? LastAttackPosition { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public Vector3? TurnStartPosition { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public float ActionPointsAttackCount { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float ActionPointsAttackCountMax { get; set; }

	public bool RecheckEquipmentRestrictionsAfterCombatEnd { get; set; }

	public float? OverrideInitiative
	{
		get
		{
			return m_OverrideInitiative ?? base.Owner.Blueprint.GetComponent<InitiativeModifier>()?.OverrideInitiative;
		}
		set
		{
			m_OverrideInitiative = value;
		}
	}

	[CanBeNull]
	public MechanicEntity LastTarget
	{
		get
		{
			return m_LastTarget;
		}
		set
		{
			if (!(m_LastTarget != value))
			{
				return;
			}
			EntityRef<MechanicEntity> prevTarget = m_LastTarget;
			m_LastTarget = value;
			using (ProfileScope.New("LastTarget change"))
			{
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<ICombatTargetChangeHandler>)delegate(ICombatTargetChangeHandler h)
				{
					h.HandleTargetChange(prevTarget);
				}, isCheckRuntime: true);
			}
		}
	}

	[CanBeNull]
	public ItemEntityWeapon LastAbilityWeapon
	{
		get
		{
			return m_LastAbilityWeapon;
		}
		private set
		{
			if (!(m_LastAbilityWeapon == value))
			{
				EntityRef<ItemEntityWeapon> prevWeapon = m_LastAbilityWeapon;
				m_LastAbilityWeapon = value;
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<ILastAbilityWeaponChangeHandler>)delegate(ILastAbilityWeaponChangeHandler h)
				{
					h.HandleLastAbilityWeaponChange(prevWeapon, value);
				}, isCheckRuntime: true);
			}
		}
	}

	public float InitiativeRoll => base.Owner.Initiative.Roll;

	public bool IsInCombat => m_InCombat;

	public bool IsEngaged
	{
		get
		{
			if (m_InCombat)
			{
				return base.Owner.GetEngagedByUnits().Any();
			}
			return false;
		}
	}

	public bool IsEngagedInRealOrVirtualPosition
	{
		get
		{
			if (Game.Instance.Controllers.VirtualPositionController == null || !Game.Instance.Controllers.VirtualPositionController.TryGetVirtualPosition(base.Owner, out var virtualPosition))
			{
				return IsEngaged;
			}
			if (m_InCombat)
			{
				return base.Owner.IsEngagedInPosition(virtualPosition);
			}
			return false;
		}
	}

	public bool CanActInCombat => m_InCombat;

	public bool CanAttackOfOpportunity
	{
		get
		{
			if (!base.Owner.Features.DisableAttacksOfOpportunity && !base.Owner.Passive && (AttacksOfOpportunityMadeThisTurnCount < MaxAttacksOfOpportunityPerRound || base.Owner.IsPlayerFaction || base.Owner.Blueprint.DifficultyType >= UnitDifficultyType.Elite) && base.Owner.CanAttack((BaseUnitEntity unit) => unit.GetThreatHand()?.Weapon))
			{
				return base.Owner.GetThreatHand()?.Weapon.AttackOfOpportunityAbility != null;
			}
			return false;
		}
	}

	private int MaxAttacksOfOpportunityPerRound => int.MaxValue;

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	private ModifiableValue MovementPointsStat => StatsContainer.GetStat(StatType.MovementPoints);

	public int MovementPointsMax
	{
		get
		{
			if (!base.Owner.Features.ImmuneToMovementPointReduction)
			{
				return MovementPointsStat;
			}
			return MovementPointsStat.CalculateFilteredModifiedValue(Modifier.IsPositive);
		}
	}

	public int ActionPointsMax => StatsContainer.GetStat(StatType.ActionPoints);

	public int InitiativeBonus => StatsContainer.GetStat(StatType.Initiative);

	protected override void OnAttachOrPrePostLoad()
	{
		StatsContainer.Register<ModifiableValueMovementPoints>(StatType.MovementPoints);
		StatsContainer.Register(StatType.ActionPoints);
		StatsContainer.Register(StatType.Initiative);
		StatsContainer.Register<ModifiableValueAttributeBonusDependent>(StatType.Defence);
	}

	public void JoinCombat(bool surprised = false)
	{
		if (m_InCombat)
		{
			return;
		}
		Clear();
		base.Owner.CombatGroup.IsInCombat.Retain();
		m_InCombat = true;
		ResetActionAndMovementPoints();
		AbstractUnitCommand current = base.Owner.Commands.Current;
		if (current != null && !SettingsRoot.Game.TurnBased.EnableTurnBasedMode && current.IsActed && !current.IsFinished)
		{
			return;
		}
		base.Owner.CachedPerceptionRoll = 0;
		ActionPoints = 0;
		DamageReceivedInCurrentFight = 0;
		if (!base.Owner.Faction.IsPlayer)
		{
			UnitMoveTo currentMoveTo = base.Owner.Commands.CurrentMoveTo;
			if (currentMoveTo != null)
			{
				ReturnPosition = currentMoveTo.Target;
				ReturnOrientation = currentMoveTo.Orientation;
			}
			else
			{
				ReturnPosition = SizePathfindingHelper.FromViewToMechanicsPosition(base.Owner, base.Owner.Position, inBattle: true).GetNearestNodeXZUnwalkable()?.Vector3Position() ?? base.Owner.CurrentNode.position;
				base.Owner.Position = ReturnPosition.Value;
				ReturnOrientation = base.Owner.DesiredOrientation;
			}
		}
		else
		{
			ReturnPosition = null;
			ReturnOrientation = null;
		}
		Surprised = surprised;
		CutsceneControlledUnit.UpdateActiveCutscene(base.Owner);
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitCombatHandler>)delegate(IUnitCombatHandler h)
		{
			h.HandleUnitJoinCombat();
		}, isCheckRuntime: true);
		if ((bool)base.Owner.View.Animator)
		{
			base.Owner.View.HandsEquipment?.SetCombatState(inCombat: true);
			if (base.Owner.View.gameObject.activeSelf)
			{
				base.Owner.View.UpdateCombatSwitch();
			}
		}
		base.Owner.Wake();
		PFLog.Default.Log(base.Owner.View, "Unit join combat: {0}", base.Owner);
		EventBus.RaiseEvent(delegate(IAnyUnitCombatHandler h)
		{
			h.HandleUnitJoinCombat(base.Owner);
		});
	}

	public void LeaveCombat()
	{
		if (!m_InCombat)
		{
			return;
		}
		m_InCombat = false;
		base.Owner.CombatGroup.IsInCombat.Release();
		ReturnToStartingPositionIfNeeded();
		Clear();
		if (!base.Owner.Destroyed)
		{
			base.Owner.Commands.InterruptAiCommands();
			CutsceneControlledUnit.UpdateActiveCutscene(base.Owner);
			if (base.Owner.View != null && base.Owner.View.Animator != null)
			{
				base.Owner.View.HandsEquipment.SetCombatState(inCombat: false);
				if (base.Owner.View.gameObject.activeSelf)
				{
					base.Owner.View.UpdateCombatSwitch();
				}
			}
		}
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitCombatHandler>)delegate(IUnitCombatHandler h)
		{
			h.HandleUnitLeaveCombat();
		}, isCheckRuntime: true);
		EventBus.RaiseEvent(delegate(IAnyUnitCombatHandler h)
		{
			h.HandleUnitLeaveCombat(base.Owner);
		});
		PFLog.Default.Log(base.Owner.View, "Unit leave combat: {0}", base.Owner);
		if (RecheckEquipmentRestrictionsAfterCombatEnd)
		{
			base.Owner.UnequipItemsWithFailedRestrictions();
			RecheckEquipmentRestrictionsAfterCombatEnd = false;
		}
	}

	public void ReturnToStartingPositionIfNeeded()
	{
		if (base.Owner.Blueprint.IsStayOnSameSpotAfterCombat || base.Owner.Commands.CurrentMoveTo != null || !ReturnPosition.HasValue || !(Vector3.Distance(ReturnPosition.Value, base.Owner.Position) > 0.1f))
		{
			return;
		}
		PathfindingService.Instance.FindPathRT_Delayed(base.Owner.MovementAgent, ReturnPosition.Value, 0.3f, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (base.Owner == null)
			{
				path.Claim(path);
				path.Release(path);
			}
			else
			{
				UnitMoveToParams cmdParams = new UnitMoveToParams(path, ReturnPosition.Value)
				{
					Orientation = ReturnOrientation,
					DoNotInterruptAfterFight = true
				};
				base.Owner.Commands.Run(cmdParams);
			}
		});
	}

	public bool CanEndTurn()
	{
		return true;
	}

	public void PrepareForNewTurn(bool isTurnBased)
	{
		LastStraightMoveLength = 0;
		LastDiagonalCount = 0;
		MovementPointsSpentThisTurn = 0f;
		MovedCellsThisTurn = 0f;
		base.Owner.Initiative.PreparationInterrupted = false;
		if (isTurnBased)
		{
			ResetActionAndMovementPoints();
			float actionPointsAttackCountMax = (ActionPointsAttackCount = 1f);
			ActionPointsAttackCountMax = actionPointsAttackCountMax;
		}
		AttacksOfOpportunityMadeThisTurnCount = 0;
		ForceMovedDistanceInCells = 0;
	}

	private void Clear()
	{
		Surprised = false;
		AttackInRoundCount = 0;
		AttackedInRoundCount = 0;
		HitInRoundCount = 0;
		GotHitInRoundCount = 0;
		ForceMovedDistanceInCells = 0;
		LastTarget = null;
		ManualTarget = null;
		LastAbilityWeapon = null;
		LastAttackPosition = null;
		StartedCombatNearEnemy = false;
		SaveMPAfterUsingNextAbility = false;
		TurnStartPosition = null;
		DamageReceivedInCurrentFight = 0;
		IsMovedThisTurn = false;
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		AttackInRoundCount = 0;
		AttackedInRoundCount = 0;
		HitInRoundCount = 0;
		GotHitInRoundCount = 0;
		IsMovedThisTurn = false;
	}

	void IWarhammerAttackHandler.HandleAttack(RulePerformAttack rule)
	{
		if (rule.Initiator == base.Owner)
		{
			HandleAttackAsInitiator(rule.ResultIsHit, rule.Ability.Weapon);
		}
		else if (rule.Target == base.Owner)
		{
			HandleAttackAsTarget(rule.ResultIsHit, rule.ResultDamageRule?.ResultValue);
		}
	}

	private void HandleAttackAsInitiator(bool isAttackHit, ItemEntityWeapon weapon)
	{
		AttackInRoundCount++;
		if (isAttackHit)
		{
			HitInRoundCount++;
		}
		if (weapon != null)
		{
			LastAbilityWeapon = weapon;
		}
		LastAttackPosition = base.Owner.Position;
	}

	private void HandleAttackAsTarget(bool isAttackHit, int? resultDamage)
	{
		AttackedInRoundCount++;
		if (isAttackHit)
		{
			GotHitInRoundCount++;
			if (resultDamage.HasValue)
			{
				DamageReceivedInCurrentFight += resultDamage.Value;
			}
		}
	}

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		IsMovedThisTurn |= command.IsMoveUnit;
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			TurnStartPosition = base.Owner.Position;
		}
	}

	public void RegisterMoveCells(float cells)
	{
		MovedCellsThisTurn += cells;
	}

	public void ResetActionAndMovementPoints()
	{
		ActionPoints = ActionPointsMax;
		MovementPoints = MovementPointsMax;
	}

	public void ResetMovementPointsCheat()
	{
		MovementPointsSpentThisTurn = 0f;
		MovedCellsThisTurn = 0f;
		MovementPoints = 100f;
	}

	public void SpendActionPoints(int value)
	{
		if (value > 0)
		{
			ActionPoints = Math.Max(0, ActionPoints - value);
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentActionPoints>)delegate(IUnitSpentActionPoints h)
			{
				h.HandleUnitSpentActionPoints(value);
			}, isCheckRuntime: true);
		}
	}

	public void SpendMovementPoints(float value)
	{
		if (!(value <= 0f))
		{
			MovementPoints = Math.Max(0f, MovementPoints - value);
			MovementPointsSpentThisTurn += value;
			RegisterMoveCells(value);
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentMovementPoints>)delegate(IUnitSpentMovementPoints h)
			{
				h.HandleUnitSpentMovementPoints(value);
			}, isCheckRuntime: true);
		}
	}

	public void SpendMovementsPointsAll()
	{
		MovementPoints = 0f;
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentMovementPoints>)delegate(IUnitSpentMovementPoints h)
		{
			h.HandleUnitSpentMovementPoints(-1f);
		}, isCheckRuntime: true);
	}

	public void GainActionPoints(int value, [CanBeNull] MechanicsContext context)
	{
		SetActionPoints(ActionPoints + Mathf.Max(0, value), context);
	}

	public void SetActionPoints(int value, [CanBeNull] MechanicsContext context)
	{
		if (value != ActionPoints && value >= 0)
		{
			ActionPoints = value;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitGainActionPoints>)delegate(IUnitGainActionPoints h)
			{
				h.HandleUnitGainActionPoints(ActionPoints, context);
			}, isCheckRuntime: true);
		}
	}

	public void GainMovementPoints(float value, [CanBeNull] MechanicsContext context)
	{
		SetMovementPoints(MovementPoints + Mathf.Max(0f, value), context);
	}

	public void SetMovementPoints(float value, [CanBeNull] MechanicsContext context)
	{
		if (!(Math.Abs(value - MovementPoints) < float.Epsilon) && !(value < 0f))
		{
			MovementPoints = value;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitGainMovementPoints>)delegate(IUnitGainMovementPoints h)
			{
				h.HandleUnitGainMovementPoints(MovementPoints, context);
			}, isCheckRuntime: true);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_InCombat);
		if (ReturnPosition.HasValue)
		{
			Vector3 val2 = ReturnPosition.Value;
			result.Append(ref val2);
		}
		if (ReturnOrientation.HasValue)
		{
			float val3 = ReturnOrientation.Value;
			result.Append(ref val3);
		}
		int val4 = AttackInRoundCount;
		result.Append(ref val4);
		int val5 = AttackedInRoundCount;
		result.Append(ref val5);
		int val6 = HitInRoundCount;
		result.Append(ref val6);
		int val7 = GotHitInRoundCount;
		result.Append(ref val7);
		bool val8 = IsMovedThisTurn;
		result.Append(ref val8);
		bool val9 = SaveMPAfterUsingNextAbility;
		result.Append(ref val9);
		bool val10 = Surprised;
		result.Append(ref val10);
		float val11 = MovementPoints;
		result.Append(ref val11);
		float val12 = MovedCellsThisTurn;
		result.Append(ref val12);
		float val13 = MovementPointsSpentThisTurn;
		result.Append(ref val13);
		int val14 = ActionPoints;
		result.Append(ref val14);
		int val15 = AttacksOfOpportunityMadeThisTurnCount;
		result.Append(ref val15);
		int val16 = ForceMovedDistanceInCells;
		result.Append(ref val16);
		int val17 = LastStraightMoveLength;
		result.Append(ref val17);
		int val18 = LastDiagonalCount;
		result.Append(ref val18);
		if (m_OverrideInitiative.HasValue)
		{
			float val19 = m_OverrideInitiative.Value;
			result.Append(ref val19);
		}
		result.Append(ref DamageReceivedInCurrentFight);
		Hash128 val20 = ClassHasher<MechanicEntity>.GetHash128(ManualTarget);
		result.Append(ref val20);
		bool val21 = StartedCombatNearEnemy;
		result.Append(ref val21);
		EntityRef<MechanicEntity> obj = m_LastTarget;
		Hash128 val22 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val22);
		EntityRef<ItemEntityWeapon> obj2 = m_LastAbilityWeapon;
		Hash128 val23 = StructHasher<EntityRef<ItemEntityWeapon>>.GetHash128(ref obj2);
		result.Append(ref val23);
		if (LastAttackPosition.HasValue)
		{
			Vector3 val24 = LastAttackPosition.Value;
			result.Append(ref val24);
		}
		if (TurnStartPosition.HasValue)
		{
			Vector3 val25 = TurnStartPosition.Value;
			result.Append(ref val25);
		}
		float val26 = ActionPointsAttackCount;
		result.Append(ref val26);
		float val27 = ActionPointsAttackCountMax;
		result.Append(ref val27);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitCombatState source = new PartUnitCombatState();
		result = Unsafe.As<PartUnitCombatState, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<PartUnitCombatState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_InCombat", ref m_InCombat, state);
		Vector3? value = ReturnPosition;
		formatter.NullableField(1, "ReturnPosition", ref value, state);
		float? value2 = ReturnOrientation;
		formatter.UnmanagedNullableField(2, "ReturnOrientation", ref value2, state);
		int value3 = AttackInRoundCount;
		formatter.UnmanagedField(3, "AttackInRoundCount", ref value3, state);
		int value4 = AttackedInRoundCount;
		formatter.UnmanagedField(4, "AttackedInRoundCount", ref value4, state);
		int value5 = HitInRoundCount;
		formatter.UnmanagedField(5, "HitInRoundCount", ref value5, state);
		int value6 = GotHitInRoundCount;
		formatter.UnmanagedField(6, "GotHitInRoundCount", ref value6, state);
		bool value7 = IsMovedThisTurn;
		formatter.UnmanagedField(7, "IsMovedThisTurn", ref value7, state);
		bool value8 = SaveMPAfterUsingNextAbility;
		formatter.UnmanagedField(8, "SaveMPAfterUsingNextAbility", ref value8, state);
		bool value9 = Surprised;
		formatter.UnmanagedField(9, "Surprised", ref value9, state);
		float value10 = MovementPoints;
		formatter.UnmanagedField(10, "MovementPoints", ref value10, state);
		float value11 = MovedCellsThisTurn;
		formatter.UnmanagedField(11, "MovedCellsThisTurn", ref value11, state);
		float value12 = MovementPointsSpentThisTurn;
		formatter.UnmanagedField(12, "MovementPointsSpentThisTurn", ref value12, state);
		int value13 = ActionPoints;
		formatter.UnmanagedField(13, "ActionPoints", ref value13, state);
		int value14 = AttacksOfOpportunityMadeThisTurnCount;
		formatter.UnmanagedField(14, "AttacksOfOpportunityMadeThisTurnCount", ref value14, state);
		int value15 = ForceMovedDistanceInCells;
		formatter.UnmanagedField(15, "ForceMovedDistanceInCells", ref value15, state);
		int value16 = LastStraightMoveLength;
		formatter.UnmanagedField(16, "LastStraightMoveLength", ref value16, state);
		int value17 = LastDiagonalCount;
		formatter.UnmanagedField(17, "LastDiagonalCount", ref value17, state);
		formatter.UnmanagedNullableField(18, "m_OverrideInitiative", ref m_OverrideInitiative, state);
		formatter.UnmanagedField(19, "DamageReceivedInCurrentFight", ref DamageReceivedInCurrentFight, state);
		MechanicEntity value18 = ManualTarget;
		formatter.Field(20, "ManualTarget", ref value18, state);
		bool value19 = StartedCombatNearEnemy;
		formatter.UnmanagedField(21, "StartedCombatNearEnemy", ref value19, state);
		formatter.Field(22, "m_LastTarget", ref m_LastTarget, state);
		formatter.Field(23, "m_LastAbilityWeapon", ref m_LastAbilityWeapon, state);
		Vector3? value20 = LastAttackPosition;
		formatter.NullableField(24, "LastAttackPosition", ref value20, state);
		Vector3? value21 = TurnStartPosition;
		formatter.NullableField(25, "TurnStartPosition", ref value21, state);
		float value22 = ActionPointsAttackCount;
		formatter.UnmanagedField(26, "ActionPointsAttackCount", ref value22, state);
		float value23 = ActionPointsAttackCountMax;
		formatter.UnmanagedField(27, "ActionPointsAttackCountMax", ref value23, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitCombatState>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_InCombat = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				ReturnPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 2:
				ReturnOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 3:
				AttackInRoundCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				AttackedInRoundCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 5:
				HitInRoundCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 6:
				GotHitInRoundCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 7:
				IsMovedThisTurn = formatter.ReadUnmanaged<bool>(state);
				break;
			case 8:
				SaveMPAfterUsingNextAbility = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				Surprised = formatter.ReadUnmanaged<bool>(state);
				break;
			case 10:
				MovementPoints = formatter.ReadUnmanaged<float>(state);
				break;
			case 11:
				MovedCellsThisTurn = formatter.ReadUnmanaged<float>(state);
				break;
			case 12:
				MovementPointsSpentThisTurn = formatter.ReadUnmanaged<float>(state);
				break;
			case 13:
				ActionPoints = formatter.ReadUnmanaged<int>(state);
				break;
			case 14:
				AttacksOfOpportunityMadeThisTurnCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 15:
				ForceMovedDistanceInCells = formatter.ReadUnmanaged<int>(state);
				break;
			case 16:
				LastStraightMoveLength = formatter.ReadUnmanaged<int>(state);
				break;
			case 17:
				LastDiagonalCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 18:
				m_OverrideInitiative = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 19:
				DamageReceivedInCurrentFight = formatter.ReadUnmanaged<int>(state);
				break;
			case 20:
				ManualTarget = formatter.ReadPackable<MechanicEntity>(state);
				break;
			case 21:
				StartedCombatNearEnemy = formatter.ReadUnmanaged<bool>(state);
				break;
			case 22:
				m_LastTarget = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 23:
				m_LastAbilityWeapon = formatter.ReadPackable<EntityRef<ItemEntityWeapon>>(state);
				break;
			case 24:
				LastAttackPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 25:
				TurnStartPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 26:
				ActionPointsAttackCount = formatter.ReadUnmanaged<float>(state);
				break;
			case 27:
				ActionPointsAttackCountMax = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
