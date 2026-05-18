using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.Gameplay.Features.Encounter;

[OwlPackable(OwlPackableMode.Generate)]
[HashNoGenerate]
public sealed class PartEncounterObjectives : MechanicEntityPart<ActiveEncounter>, IEtudesUpdateHandler, ISubscriber, IUnlockValueHandler, IOwlPackable<PartEncounterObjectives>
{
	[JsonProperty]
	[OwlPackInclude]
	private EncounterObjectiveState[] _states = Array.Empty<EncounterObjectiveState>();

	[JsonProperty]
	[OwlPackInclude]
	private int[] _lastCounterValues = Array.Empty<int>();

	private EncounterObjectivesComponent? _component;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartEncounterObjectives",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("_states", typeof(EncounterObjectiveState[])),
			new FieldInfo("_lastCounterValues", typeof(int[]))
		}
	};

	[JsonConstructor]
	public PartEncounterObjectives()
	{
	}

	protected override void OnAttach()
	{
		if (base.Owner.Blueprint.TryGetComponent<EncounterObjectivesComponent>(out _component))
		{
			int num = _component.Objectives.Length;
			_states = new EncounterObjectiveState[num];
			_lastCounterValues = new int[num];
			for (int i = 0; i < num; i++)
			{
				_states[i] = (_component.Objectives[i].ActiveFromStart ? EncounterObjectiveState.Active : EncounterObjectiveState.Inactive);
			}
			InitCounters();
			UpdateObjectiveStates(raiseEvents: false);
		}
	}

	protected override void OnPostLoad()
	{
		if (base.Owner.Blueprint.TryGetComponent<EncounterObjectivesComponent>(out _component))
		{
			InitCounters();
		}
	}

	public IReadOnlyList<EncounterObjectiveInfo> GetObjectives()
	{
		if (_component == null)
		{
			return Array.Empty<EncounterObjectiveInfo>();
		}
		EncounterObjectiveInfo[] array = new EncounterObjectiveInfo[_component.Objectives.Length];
		for (int i = 0; i < _component.Objectives.Length; i++)
		{
			EncounterObjective encounterObjective = _component.Objectives[i];
			array[i] = new EncounterObjectiveInfo(encounterObjective.Description, encounterObjective.Hint, encounterObjective.Type, _states[i], encounterObjective.ActiveStateResolution, _lastCounterValues[i], encounterObjective.TargetValue?.GetValue(), encounterObjective.CounterFormat);
		}
		return array;
	}

	void IEtudesUpdateHandler.OnEtudesUpdate()
	{
		UpdateObjectiveStates(raiseEvents: true);
	}

	void IUnlockValueHandler.HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		UpdateCounters();
	}

	private void UpdateObjectiveStates(bool raiseEvents)
	{
		if (_component != null)
		{
			for (int i = 0; i < _component.Objectives.Length; i++)
			{
				UpdateObjectiveState(i, raiseEvents);
			}
		}
	}

	private void UpdateObjectiveState(int index, bool raiseEvents)
	{
		EncounterObjective objective = _component.Objectives[index];
		EncounterObjectiveState encounterObjectiveState = _states[index];
		EncounterObjectiveState newState = GetNewState(encounterObjectiveState, objective);
		if (newState != encounterObjectiveState)
		{
			_states[index] = newState;
			if (raiseEvents)
			{
				RaiseStateChanged(index, newState);
			}
		}
	}

	private static EncounterObjectiveState GetNewState(EncounterObjectiveState currentState, EncounterObjective objective)
	{
		bool flag = currentState == EncounterObjectiveState.Completed || currentState == EncounterObjectiveState.Failed;
		if (flag && !objective.CanExitFromFinalState)
		{
			return currentState;
		}
		if (currentState == EncounterObjectiveState.Inactive)
		{
			if (!ConditionsPassed(objective.ActivationCondition))
			{
				return EncounterObjectiveState.Inactive;
			}
			return EncounterObjectiveState.Active;
		}
		if (flag && ConditionsPassed(objective.ActivationCondition))
		{
			return EncounterObjectiveState.Active;
		}
		if (ConditionsPassed(objective.FailureCondition))
		{
			return EncounterObjectiveState.Failed;
		}
		if (ConditionsPassed(objective.CompletionCondition))
		{
			return EncounterObjectiveState.Completed;
		}
		if (ConditionsPassed(objective.DeactivationCondition))
		{
			return EncounterObjectiveState.Inactive;
		}
		return currentState;
	}

	private static bool ConditionsPassed(ConditionsChecker conditionsChecker)
	{
		if (conditionsChecker.HasConditions)
		{
			return conditionsChecker.Check();
		}
		return false;
	}

	private void InitCounters()
	{
		for (int i = 0; i < _component.Objectives.Length && i < _lastCounterValues.Length; i++)
		{
			EncounterObjective encounterObjective = _component.Objectives[i];
			if (encounterObjective.Type == EncounterObjectiveType.Counter)
			{
				int? num = encounterObjective.CurrentValue?.GetValue();
				if (num.HasValue)
				{
					int valueOrDefault = num.GetValueOrDefault();
					_lastCounterValues[i] = valueOrDefault;
				}
			}
		}
	}

	private void UpdateCounters()
	{
		if (_component == null)
		{
			return;
		}
		for (int i = 0; i < _component.Objectives.Length; i++)
		{
			EncounterObjective encounterObjective = _component.Objectives[i];
			if (encounterObjective.Type != EncounterObjectiveType.Counter || _states[i] != EncounterObjectiveState.Active)
			{
				continue;
			}
			int? num = encounterObjective.CurrentValue?.GetValue();
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault != _lastCounterValues[i])
				{
					_lastCounterValues[i] = valueOrDefault;
					RaiseCounterChanged(i, valueOrDefault, encounterObjective.TargetValue?.GetValue());
				}
			}
		}
	}

	private void RaiseStateChanged(int index, EncounterObjectiveState state)
	{
		base.EventBus.RaiseEvent(delegate(IEncounterObjectiveHandler h)
		{
			h.HandleObjectiveStateChanged(index, state);
		});
	}

	private void RaiseCounterChanged(int index, int newValue, int? targetValue)
	{
		base.EventBus.RaiseEvent(delegate(IEncounterObjectiveHandler h)
		{
			h.HandleObjectiveCounterChanged(index, newValue, targetValue);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartEncounterObjectives source = new PartEncounterObjectives();
		result = Unsafe.As<PartEncounterObjectives, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartEncounterObjectives>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_states", ref _states, state);
		formatter.Field(1, "_lastCounterValues", ref _lastCounterValues, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartEncounterObjectives>();
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
				_states = formatter.ReadPackable<EncounterObjectiveState[]>(state);
				break;
			case 1:
				_lastCounterValues = formatter.ReadPackable<int[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
