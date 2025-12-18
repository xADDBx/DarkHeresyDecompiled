using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[OwlPackable(OwlPackableMode.Generate)]
public class TurnOrderQueue : IHashable, IOwlPackable, IOwlPackable<TurnOrderQueue>
{
	private EntityRef<MechanicEntity> m_CurrentUnit;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TurnOrderQueue",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("CurrentTurnType", typeof(CombatTurnType)),
			new FieldInfo("RoamingUnitsTurnEndTime", typeof(TimeSpan?))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public CombatTurnType CurrentTurnType { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan? RoamingUnitsTurnEndTime { get; private set; }

	public IEnumerable<MechanicEntity> InterruptingTurnOrder => from i in UnitsInCombat
		where i.Initiative.InterruptingOrder > 0
		orderby i.Initiative.InterruptingOrder descending
		select i;

	public IEnumerable<MechanicEntity> UnitsOrder => UnitsInCombat.OrderByDescending((MechanicEntity i) => i.Initiative.TurnOrderPriority);

	public IEnumerable<MechanicEntity> CurrentRoundUnitsOrder => InterruptingTurnOrder.Concat(UnitsOrder.Where((MechanicEntity i) => !i.Initiative.ActedThisRound));

	public IEnumerable<MechanicEntity> NextRoundUnitsOrder => UnitsOrder.Where((MechanicEntity i) => i.Initiative.ActedThisRound);

	private static IEnumerable<MechanicEntity> UnitsInCombat
	{
		get
		{
			if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				return Enumerable.Empty<MechanicEntity>();
			}
			return Game.Instance.Controllers.TurnController.AllUnits.Where((MechanicEntity i) => i.IsInCombat);
		}
	}

	[CanBeNull]
	public MechanicEntity CurrentUnit
	{
		get
		{
			return m_CurrentUnit;
		}
		private set
		{
			m_CurrentUnit = value;
		}
	}

	public bool IsRoamingUnitsTurn
	{
		get
		{
			if (RoamingUnitsTurnEndTime.HasValue)
			{
				TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
				TimeSpan? roamingUnitsTurnEndTime = RoamingUnitsTurnEndTime;
				return gameTime < roamingUnitsTurnEndTime;
			}
			return false;
		}
	}

	[JsonConstructor]
	public TurnOrderQueue()
	{
	}

	public void Clear()
	{
		CurrentUnit = null;
		RoamingUnitsTurnEndTime = null;
	}

	public bool IsEmpty()
	{
		if (CurrentUnit == null && UnitsOrder.Empty())
		{
			return InterruptingTurnOrder.Empty();
		}
		return false;
	}

	public void RestoreCurrentUnit()
	{
		NextTurn(out var _, out var _);
	}

	public MechanicEntity NextTurn(out bool nextRound, out CombatTurnType turnType)
	{
		nextRound = false;
		turnType = CurrentTurnType;
		if (turnType == CombatTurnType.ManualCombat)
		{
			int num;
			if (!UnitsOrder.Any((MechanicEntity i) => i.IsPlayerEnemy))
			{
				num = (int)turnType;
			}
			else
			{
				CombatTurnType combatTurnType2 = (CurrentTurnType = CombatTurnType.Preparation);
				num = (int)combatTurnType2;
			}
			turnType = (CombatTurnType)num;
			return CurrentUnit = null;
		}
		if (turnType == CombatTurnType.Preparation)
		{
			return CurrentUnit = null;
		}
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		TimeSpan? roamingUnitsTurnEndTime = RoamingUnitsTurnEndTime;
		bool flag = gameTime >= roamingUnitsTurnEndTime;
		if (turnType == CombatTurnType.Roaming)
		{
			if (flag)
			{
				EndRoamingUnitsTurn();
				turnType = CurrentTurnType;
			}
			if (turnType == CombatTurnType.Roaming)
			{
				return CurrentUnit = null;
			}
		}
		CalculateCurrentUnit();
		if (CurrentUnit != null)
		{
			return CurrentUnit;
		}
		nextRound = true;
		turnType = CurrentTurnType;
		return CurrentUnit = null;
	}

	private void CalculateCurrentUnit()
	{
		MechanicEntity mechanicEntity = CurrentRoundUnitsOrder.FirstOrDefault();
		if (mechanicEntity == null)
		{
			CurrentUnit = null;
		}
		else if (mechanicEntity is IInitiativeDelegate { Delegate: { } @delegate })
		{
			CurrentUnit = @delegate;
			mechanicEntity.Initiative.LastTurn = Game.Instance.Controllers.TurnController.GameRound;
		}
		else
		{
			CurrentUnit = mechanicEntity;
		}
	}

	public void InterruptCurrentUnit(MechanicEntity interruptingUnit)
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			if (CurrentUnit == null)
			{
				PFLog.Default.ErrorWithReport($"Unit {interruptingUnit} can't interrupt turn because CurrentUnit is null");
				return;
			}
			if (!interruptingUnit.IsInCombat)
			{
				PFLog.Default.ErrorWithReport($"Interrupting unit {interruptingUnit} is not in combat");
				return;
			}
			if (interruptingUnit.Initiative.InterruptingOrder > 0)
			{
				PFLog.Default.ErrorWithReport($"Unit {interruptingUnit} can't interrupt turn while interrupting turn");
				return;
			}
			interruptingUnit.Initiative.InterruptingOrder = InterruptingTurnOrder.Count() + 1;
			CurrentUnit = interruptingUnit;
		}
	}

	private void BeginRoamingUnitsTurn()
	{
		RoamingUnitsTurnEndTime = Game.Instance.Controllers.TimeController.GameTime + 1.Rounds().Seconds;
		CurrentTurnType = CombatTurnType.Roaming;
		EventBus.RaiseEvent(delegate(IRoamingTurnBeginHandler h)
		{
			h.HandleBeginRoamingTurn();
		});
	}

	public void EndRoamingUnitsTurn()
	{
		RoamingUnitsTurnEndTime = null;
		CurrentTurnType = CombatTurnType.Default;
		EventBus.RaiseEvent(delegate(IRoamingTurnEndHandler h)
		{
			h.HandleEndRoamingTurn();
		});
	}

	public void BeginPreparationTurn()
	{
		CurrentTurnType = CombatTurnType.Preparation;
	}

	public void EndPreparationTurn()
	{
		CurrentTurnType = CombatTurnType.Default;
	}

	public void BeginManualCombat()
	{
		CurrentTurnType = CombatTurnType.ManualCombat;
	}

	public void EndManualCombat()
	{
		CurrentTurnType = CombatTurnType.Default;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		CombatTurnType val = CurrentTurnType;
		result.Append(ref val);
		if (RoamingUnitsTurnEndTime.HasValue)
		{
			TimeSpan val2 = RoamingUnitsTurnEndTime.Value;
			result.Append(ref val2);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TurnOrderQueue source = new TurnOrderQueue();
		result = Unsafe.As<TurnOrderQueue, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<TurnOrderQueue>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CombatTurnType value = CurrentTurnType;
		formatter.EnumField(0, "CurrentTurnType", ref value, state);
		TimeSpan? value2 = RoamingUnitsTurnEndTime;
		formatter.NullableField(1, "RoamingUnitsTurnEndTime", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TurnOrderQueue>();
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
				CurrentTurnType = formatter.ReadEnum<CombatTurnType>(state);
				break;
			case 1:
				RoamingUnitsTurnEndTime = formatter.ReadNullablePackable<TimeSpan>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
