using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Squads;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[OwlPackable(OwlPackableMode.Generate)]
public class Initiative : IHashable, IOwlPackable, IOwlPackable<Initiative>
{
	public enum Event
	{
		RoundStart,
		RoundEnd,
		TurnStart,
		TurnEnd
	}

	public readonly struct BulkSwapFluent
	{
		private readonly Initiative _self;

		public BulkSwapFluent(Initiative self)
		{
			_self = self;
		}

		public BulkSwapFluent Swap(Initiative src)
		{
			_self.Swap(src);
			return this;
		}

		public void Finish()
		{
			EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
			{
				h.HandleInitiativeChanged();
			});
		}
	}

	public const float Min = 1f;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Initiative",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("Roll", typeof(float)),
			new FieldInfo("Value", typeof(float)),
			new FieldInfo("Order", typeof(int)),
			new FieldInfo("InterruptingOrder", typeof(int)),
			new FieldInfo("WasPreparedForRound", typeof(int)),
			new FieldInfo("PreparationInterrupted", typeof(bool)),
			new FieldInfo("LastTurn", typeof(int))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public float Roll { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public float Value { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int Order { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int InterruptingOrder { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int WasPreparedForRound { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool PreparationInterrupted { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int LastTurn { get; set; }

	public double TurnOrderPriority => (double)Value + 0.001 - (double)Order * 0.0001;

	public bool ActedThisRound => LastTurn == Game.Instance.Controllers.TurnController.GameRound;

	public bool Empty => Value == 0f;

	public void Clear()
	{
		Value = 0f;
		Order = 0;
		InterruptingOrder = 0;
		Roll = 0f;
		WasPreparedForRound = 0;
	}

	public bool ShouldActNow(bool isTurnBased, Event @event, out int actRound)
	{
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			actRound = 0;
			return false;
		}
		int gameRound = Game.Instance.Controllers.TurnController.GameRound;
		if (!isTurnBased)
		{
			actRound = ((@event == Event.RoundStart) ? gameRound : 0);
		}
		else if (LastTurn >= gameRound)
		{
			actRound = 0;
		}
		else
		{
			switch (@event)
			{
			case Event.RoundStart:
				actRound = 0;
				break;
			case Event.RoundEnd:
				actRound = gameRound;
				break;
			default:
			{
				if (Game.Instance.Controllers.TurnController.CurrentUnit is UnitSquad unitSquad)
				{
					double num = unitSquad.Units.Min((UnitReference u) => u.ToBaseUnitEntity().Initiative.TurnOrderPriority);
					actRound = ((TurnOrderPriority >= num) ? gameRound : 0);
					break;
				}
				MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
				if (currentUnit != null)
				{
					actRound = ((TurnOrderPriority >= currentUnit.Initiative.TurnOrderPriority) ? gameRound : 0);
					break;
				}
				actRound = 0;
				TurnController turnController = Game.Instance.Controllers.TurnController;
				if ((turnController == null || (!turnController.IsPreparationTurn && !turnController.IsManualCombatTurn)) ? true : false)
				{
					PFLog.Default.ErrorWithReport("Something wrong with initiative of buff or area effect");
				}
				break;
			}
			}
		}
		return actRound > 0;
	}

	public static int OrderAscending(Initiative i1, Initiative i2)
	{
		return i1.TurnOrderPriority.CompareTo(i2.TurnOrderPriority);
	}

	public static int OrderDescending(Initiative i1, Initiative i2)
	{
		return -OrderAscending(i1, i2);
	}

	public static int OrderAscending(IInitiativeHolder i1, IInitiativeHolder i2)
	{
		return OrderAscending(i1.Initiative, i2.Initiative);
	}

	public static int OrderDescending(IInitiativeHolder i1, IInitiativeHolder i2)
	{
		return OrderDescending(i1.Initiative, i2.Initiative);
	}

	public void CopyFrom(Initiative initiative)
	{
		Roll = initiative.Roll;
		Value = initiative.Value;
		Order = initiative.Order;
	}

	public void SwapPlaces(Initiative initiative)
	{
		if (initiative != this)
		{
			Swap(initiative);
			EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
			{
				h.HandleInitiativeChanged();
			});
		}
	}

	private void Swap(Initiative initiative)
	{
		float value = initiative.Value;
		float value2 = Value;
		float num2 = (Value = value);
		num2 = (initiative.Value = value2);
		int order = initiative.Order;
		int order2 = Order;
		int num5 = (Order = order);
		num5 = (initiative.Order = order2);
	}

	public BulkSwapFluent BulkSwapStart()
	{
		return new BulkSwapFluent(this);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		float val = Roll;
		result.Append(ref val);
		float val2 = Value;
		result.Append(ref val2);
		int val3 = Order;
		result.Append(ref val3);
		int val4 = InterruptingOrder;
		result.Append(ref val4);
		int val5 = WasPreparedForRound;
		result.Append(ref val5);
		bool val6 = PreparationInterrupted;
		result.Append(ref val6);
		int val7 = LastTurn;
		result.Append(ref val7);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Initiative source = new Initiative();
		result = Unsafe.As<Initiative, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Initiative>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		float value = Roll;
		formatter.UnmanagedField(0, "Roll", ref value, state);
		float value2 = Value;
		formatter.UnmanagedField(1, "Value", ref value2, state);
		int value3 = Order;
		formatter.UnmanagedField(2, "Order", ref value3, state);
		int value4 = InterruptingOrder;
		formatter.UnmanagedField(3, "InterruptingOrder", ref value4, state);
		int value5 = WasPreparedForRound;
		formatter.UnmanagedField(4, "WasPreparedForRound", ref value5, state);
		bool value6 = PreparationInterrupted;
		formatter.UnmanagedField(5, "PreparationInterrupted", ref value6, state);
		int value7 = LastTurn;
		formatter.UnmanagedField(6, "LastTurn", ref value7, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Initiative>();
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
				Roll = formatter.ReadUnmanaged<float>(state);
				break;
			case 1:
				Value = formatter.ReadUnmanaged<float>(state);
				break;
			case 2:
				Order = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				InterruptingOrder = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				WasPreparedForRound = formatter.ReadUnmanaged<int>(state);
				break;
			case 5:
				PreparationInterrupted = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				LastTurn = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
