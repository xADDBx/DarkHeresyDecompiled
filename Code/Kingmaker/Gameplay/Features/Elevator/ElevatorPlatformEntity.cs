using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class ElevatorPlatformEntity : MapObjectEntity, IHashable, IOwlPackable<ElevatorPlatformEntity>
{
	[OwlPackInclude]
	private EntityRef<ElevatorPlatformStopEntity?> _currentStop;

	[OwlPackInclude]
	private EntityRef<ElevatorPlatformStopEntity?> _targetStop;

	[OwlPackInclude]
	private ElevatorPlatformState _state;

	[OwlPackInclude]
	private int _nextWaypointIndex;

	[OwlPackInclude]
	private float _rotationOnWaypointTime;

	[OwlPackInclude]
	private ElevatorPlatformPassenger[]? _passengers;

	[OwlPackInclude]
	private EntityRef<CutscenePlayerData?> _cutscene;

	[OwlPackInclude]
	public readonly CountableFlag CutsceneHold = new CountableFlag();

	private ElevatorPlatformRoute? _transitionRoute;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ElevatorPlatformEntity",
		OldNames = null,
		Fields = new FieldInfo[25]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("WasHighlightedOnRevealAndNoticed", typeof(bool)),
			new FieldInfo("ViewSettings", typeof(MapObjectViewSettings)),
			new FieldInfo("IsNewInGame", typeof(bool)),
			new FieldInfo("_currentStop", typeof(EntityRef<ElevatorPlatformStopEntity>)),
			new FieldInfo("_targetStop", typeof(EntityRef<ElevatorPlatformStopEntity>)),
			new FieldInfo("_state", typeof(ElevatorPlatformState)),
			new FieldInfo("_nextWaypointIndex", typeof(int)),
			new FieldInfo("_rotationOnWaypointTime", typeof(float)),
			new FieldInfo("_passengers", typeof(ElevatorPlatformPassenger[])),
			new FieldInfo("_cutscene", typeof(EntityRef<CutscenePlayerData>)),
			new FieldInfo("CutsceneHold", typeof(CountableFlag))
		}
	};

	public override bool SetTransformFromConfigOnLoad => false;

	public override bool SetViewTransform => false;

	public new IElevatorPlatformConfig Config => (base.View as IElevatorPlatformConfig) ?? throw new InvalidOperationException();

	public ElevatorPlatformStopEntity CurrentStop => _currentStop.Entity ?? throw new InvalidOperationException();

	public bool IsIdle => _state == ElevatorPlatformState.Idle;

	public ElevatorPlatformEntity(IElevatorPlatformConfig config)
		: base(config)
	{
	}

	private ElevatorPlatformEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView CreateViewForData()
	{
		return null;
	}

	protected override void OnAreaLoadingComplete()
	{
		base.OnAreaLoadingComplete();
		EnsureCoherence();
	}

	protected override void OnPositionChanged()
	{
		base.OnPositionChanged();
	}

	protected override void OnOrientationChanged()
	{
		base.OnOrientationChanged();
	}

	public void StartTransition(ElevatorPlatformStopEntity destination, BlueprintCutscene? cutscene = null)
	{
		if (cutscene == null)
		{
			cutscene = Config.Cutscene ?? ConfigRoot.Instance.CutsceneRoot.DefaultElevatorCutscene;
		}
		if (cutscene != null)
		{
			if (!cutscene.LockControl)
			{
				throw new InvalidOperationException($"Elevator cutscene {cutscene} must have LockControl=true");
			}
			ParametrizedContextSetter context = new ParametrizedContextSetter
			{
				AdditionalParams = { 
				{
					"Elevator",
					(INamedParameterValue)new NamedParameterValue_MapObject(this)
				} }
			};
			_cutscene = CutscenePlayerView.Play(cutscene, context, queued: false, HoldingState).PlayerData;
		}
		StartTransitionWithoutCutscene(destination);
	}

	public void StartTransitionWithoutCutscene(ElevatorPlatformStopEntity destination)
	{
		EnsureCoherence();
		PrepareTransition(destination);
	}

	public void ForceComplete()
	{
		if (_state == ElevatorPlatformState.Idle)
		{
			return;
		}
		ElevatorPlatformStopEntity entity = _targetStop.Entity;
		if (entity != null)
		{
			base.Position = entity.Position;
			base.Orientation = entity.Orientation;
			_currentStop = entity;
			if (_passengers != null && _transitionRoute != null)
			{
				UpdatePassengers();
			}
		}
		ClearTransition();
	}

	private void PrepareTransition(ElevatorPlatformStopEntity destination)
	{
		if (_state != 0)
		{
			throw new InvalidOperationException("Can't start new elevator transition while another transition is in progress");
		}
		if (CurrentStop == destination)
		{
			throw new InvalidOperationException();
		}
		IElevatorPlatformRouteSettings elevatorPlatformRouteSettings = FindRoute(destination) ?? throw new InvalidOperationException($"Can't find route for elevator {this} from {CurrentStop} to {destination}");
		_targetStop = destination;
		_state = ElevatorPlatformState.PrepareToTransition;
		_transitionRoute = new ElevatorPlatformRoute(elevatorPlatformRouteSettings, CurrentStop == elevatorPlatformRouteSettings.To);
		_nextWaypointIndex = 1;
	}

	public void Update()
	{
		if (_state != 0)
		{
			if (_transitionRoute == null)
			{
				throw new NullReferenceException();
			}
			if (_nextWaypointIndex < 1 || _nextWaypointIndex >= _transitionRoute.Waypoints.Length)
			{
				throw new IndexOutOfRangeException();
			}
			if (_nextWaypointIndex >= _transitionRoute.Waypoints.Length)
			{
				throw new IndexOutOfRangeException();
			}
			ElevatorPlatformTransform currentWaypoint = _transitionRoute.Waypoints[_nextWaypointIndex - 1];
			ElevatorPlatformTransform nextWaypoint = _transitionRoute.Waypoints[_nextWaypointIndex];
			UpdateTransition(currentWaypoint, nextWaypoint);
		}
	}

	private void UpdateTransition(ElevatorPlatformTransform currentWaypoint, ElevatorPlatformTransform nextWaypoint)
	{
		GameMode currentGameMode = Game.Instance.CurrentGameMode;
		float deltaTime = Game.Instance.Controllers.TimeController.DeltaTime;
		CutscenePlayerData entity = _cutscene.Entity;
		if ((entity != null && !entity.IsFinished && currentGameMode?.Type != GameModeType.Cutscene) || (bool)CutsceneHold)
		{
			return;
		}
		Vector3 normalized = (nextWaypoint.Position - currentWaypoint.Position).normalized;
		ElevatorPlatformTransitionSettings elevatorPlatformTransitionSettings = _transitionRoute.Transitions[_nextWaypointIndex - 1];
		float t;
		Vector3 vector = ProjectPointOnSegment(base.Position, currentWaypoint.Position, nextWaypoint.Position, out t);
		float num = elevatorPlatformTransitionSettings.MovementSpeedCurve.Evaluate(t);
		float movementSpeed = elevatorPlatformTransitionSettings.MovementSpeed;
		float num2 = Math.Clamp(movementSpeed * num, 0.5f, movementSpeed);
		Vector3 position = ProjectPointOnSegment(vector + normalized * num2 * deltaTime, currentWaypoint.Position, nextWaypoint.Position, out t);
		bool flag;
		do
		{
			flag = false;
			ElevatorPlatformState state = _state;
			switch (_state)
			{
			case ElevatorPlatformState.Idle:
				_currentStop = ((CurrentStop == _transitionRoute.From) ? _transitionRoute.To : _transitionRoute.From);
				ClearTransition();
				break;
			case ElevatorPlatformState.PrepareToTransition:
			{
				_passengers = CollectPassengers().ToArray();
				ElevatorPlatformPassenger[] passengers = _passengers;
				foreach (ElevatorPlatformPassenger elevatorPlatformPassenger in passengers)
				{
					if (elevatorPlatformPassenger.Entity is AbstractUnitEntity abstractUnitEntity)
					{
						abstractUnitEntity.Features.OnElevator.Retain();
					}
				}
				_state = ElevatorPlatformState.StartTransitionToWaypoint;
				flag = true;
				break;
			}
			case ElevatorPlatformState.StartTransitionToWaypoint:
				_state = ((elevatorPlatformTransitionSettings.Rotation == ElevatorPlatformRotationType.BeforeMove) ? ElevatorPlatformState.RotateOnWaypoint : ElevatorPlatformState.MoveToWaypoint);
				flag = true;
				break;
			case ElevatorPlatformState.MoveToWaypoint:
				base.Position = position;
				if (elevatorPlatformTransitionSettings.Rotation == ElevatorPlatformRotationType.WhileMoving)
				{
					base.Orientation = Mathf.LerpAngle(currentWaypoint.Orientation, nextWaypoint.Orientation, elevatorPlatformTransitionSettings.RotationCurve.Evaluate(t));
				}
				if (t >= 1f)
				{
					_state = ((elevatorPlatformTransitionSettings.Rotation == ElevatorPlatformRotationType.AfterMove) ? ElevatorPlatformState.RotateOnWaypoint : ElevatorPlatformState.EndTransitionToWaypoint);
					flag = true;
				}
				UpdatePassengers();
				break;
			case ElevatorPlatformState.RotateOnWaypoint:
			{
				_rotationOnWaypointTime += deltaTime;
				float num3 = Mathf.Clamp01(_rotationOnWaypointTime / elevatorPlatformTransitionSettings.RotationDuration);
				base.Orientation = Mathf.LerpAngle(currentWaypoint.Orientation, nextWaypoint.Orientation, elevatorPlatformTransitionSettings.RotationCurve.Evaluate(num3));
				if (num3 >= 1f)
				{
					_state = elevatorPlatformTransitionSettings.Rotation switch
					{
						ElevatorPlatformRotationType.AfterMove => ElevatorPlatformState.EndTransitionToWaypoint, 
						ElevatorPlatformRotationType.BeforeMove => ElevatorPlatformState.MoveToWaypoint, 
						_ => throw new ArgumentOutOfRangeException(), 
					};
					flag = true;
				}
				UpdatePassengers();
				break;
			}
			case ElevatorPlatformState.EndTransitionToWaypoint:
				base.Position = nextWaypoint.Position;
				base.Orientation = nextWaypoint.Orientation;
				_nextWaypointIndex++;
				_rotationOnWaypointTime = 0f;
				if (_nextWaypointIndex >= _transitionRoute.Waypoints.Length)
				{
					_state = ElevatorPlatformState.Idle;
					flag = true;
				}
				else
				{
					_state = ElevatorPlatformState.StartTransitionToWaypoint;
				}
				UpdatePassengers();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			_ = _state;
		}
		while (flag);
	}

	private void UpdatePassengers()
	{
		ElevatorPlatformTransform elevatorPlatformTransform = _transitionRoute.Waypoints[0];
		ElevatorPlatformPassenger[] passengers = _passengers;
		for (int i = 0; i < passengers.Length; i++)
		{
			ElevatorPlatformPassenger elevatorPlatformPassenger = passengers[i];
			MechanicEntity entity = elevatorPlatformPassenger.Entity;
			if (entity != null)
			{
				MechanicEntity mechanicEntity = entity;
				(Vector3, float) positionAndOrientationForChild = Entity.GetPositionAndOrientationForChild(elevatorPlatformTransform.Position, elevatorPlatformTransform.Orientation, base.Position, base.Orientation, elevatorPlatformPassenger.InitialPosition, elevatorPlatformPassenger.InitialOrientation);
				(mechanicEntity.Position, entity.Orientation) = positionAndOrientationForChild;
				if (entity is AbstractUnitEntity abstractUnitEntity)
				{
					abstractUnitEntity.DesiredOrientation = abstractUnitEntity.Orientation;
				}
			}
		}
	}

	private void EnsureCoherence()
	{
		if (_currentStop.Entity == null)
		{
			if (!_currentStop.IsNull)
			{
				PFLog.Default.Error($"Current stop of elevator {this} is missing");
			}
			ElevatorPlatformStopEntity elevatorPlatformStopEntity = Config.Stops.Dereference().NotNull().MinBy((ElevatorPlatformStopEntity i) => Vector3.Distance(i.Position, base.Position));
			if (elevatorPlatformStopEntity != null)
			{
				_currentStop = elevatorPlatformStopEntity;
				base.Position = elevatorPlatformStopEntity.Position;
				base.Orientation = elevatorPlatformStopEntity.Orientation;
			}
			else
			{
				PFLog.Default.Error($"Not found suitable stop for elevator platform {this}");
			}
		}
		if (_state == ElevatorPlatformState.Idle)
		{
			return;
		}
		if (_currentStop.Entity != null)
		{
			ElevatorPlatformStopEntity entity = _targetStop.Entity;
			if (entity != null)
			{
				IElevatorPlatformRouteSettings elevatorPlatformRouteSettings = FindRoute(entity);
				if (elevatorPlatformRouteSettings != null)
				{
					_transitionRoute = new ElevatorPlatformRoute(elevatorPlatformRouteSettings, CurrentStop == elevatorPlatformRouteSettings.To);
					return;
				}
			}
			PFLog.Default.Error($"Can't find route for elevator {this} from {_currentStop.Entity} to {_targetStop.Entity}");
			if (entity != null)
			{
				base.Position = entity.Position;
				base.Orientation = entity.Orientation;
				_currentStop = entity;
			}
			ClearTransition();
		}
		else
		{
			ClearTransition();
		}
	}

	private void ClearTransition()
	{
		_state = ElevatorPlatformState.Idle;
		_transitionRoute = null;
		_nextWaypointIndex = 0;
		_rotationOnWaypointTime = 0f;
		_targetStop = null;
		_cutscene = null;
		if (_passengers != null)
		{
			ElevatorPlatformPassenger[] passengers = _passengers;
			foreach (ElevatorPlatformPassenger elevatorPlatformPassenger in passengers)
			{
				if (elevatorPlatformPassenger.Entity is AbstractUnitEntity abstractUnitEntity)
				{
					abstractUnitEntity.Features.OnElevator.ReleaseAll();
				}
			}
		}
		_passengers = null;
	}

	private static Vector3 ProjectPointOnSegment(Vector3 p, Vector3 a, Vector3 b, out float t)
	{
		Vector3 vector = b - a;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < 1E-08f)
		{
			t = 0f;
			return a;
		}
		t = Mathf.Clamp01(Vector3.Dot(p - a, vector) / sqrMagnitude);
		return a + t * vector;
	}

	private IElevatorPlatformRouteSettings? FindRoute(ElevatorPlatformStopEntity destination)
	{
		ElevatorPlatformStopEntity destination = destination;
		return Config.Routes.FirstOrDefault((IElevatorPlatformRouteSettings i) => i.From == CurrentStop && i.To == destination) ?? Config.Routes.FirstOrDefault((IElevatorPlatformRouteSettings i) => i.From == destination && i.To == CurrentStop);
	}

	private IEnumerable<ElevatorPlatformPassenger> CollectPassengers()
	{
		Bounds bounds = new Bounds(base.Position, (Config.Size + Vector2.one / 2f).To3D(1000f));
		foreach (MechanicEntity mechanicEntity in Game.Instance.EntityPools.MechanicEntities)
		{
			if (mechanicEntity != this && (mechanicEntity is AbstractUnitEntity || mechanicEntity is DroppedLootEntity) && bounds.Contains(mechanicEntity.Position))
			{
				yield return new ElevatorPlatformPassenger(mechanicEntity);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ElevatorPlatformEntity source = new ElevatorPlatformEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<ElevatorPlatformEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ElevatorPlatformEntity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		bool value3 = base.WasHighlightedOnRevealAndNoticed;
		formatter.UnmanagedField(14, "WasHighlightedOnRevealAndNoticed", ref value3, state);
		MapObjectViewSettings value4 = base.ViewSettings;
		formatter.Field(15, "ViewSettings", ref value4, state);
		bool value5 = base.IsNewInGame;
		formatter.UnmanagedField(16, "IsNewInGame", ref value5, state);
		formatter.Field(17, "_currentStop", ref _currentStop, state);
		formatter.Field(18, "_targetStop", ref _targetStop, state);
		formatter.EnumField(19, "_state", ref _state, state);
		formatter.UnmanagedField(20, "_nextWaypointIndex", ref _nextWaypointIndex, state);
		formatter.UnmanagedField(21, "_rotationOnWaypointTime", ref _rotationOnWaypointTime, state);
		formatter.Field(22, "_passengers", ref _passengers, state);
		formatter.Field(23, "_cutscene", ref _cutscene, state);
		CountableFlag value6 = CutsceneHold;
		formatter.Field(24, "CutsceneHold", ref value6, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ElevatorPlatformEntity>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				base.WasHighlightedOnRevealAndNoticed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				base.ViewSettings = formatter.ReadPackable<MapObjectViewSettings>(state);
				break;
			case 16:
				base.IsNewInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 17:
				_currentStop = formatter.ReadPackable<EntityRef<ElevatorPlatformStopEntity>>(state);
				break;
			case 18:
				_targetStop = formatter.ReadPackable<EntityRef<ElevatorPlatformStopEntity>>(state);
				break;
			case 19:
				_state = formatter.ReadEnum<ElevatorPlatformState>(state);
				break;
			case 20:
				_nextWaypointIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 21:
				_rotationOnWaypointTime = formatter.ReadUnmanaged<float>(state);
				break;
			case 22:
				_passengers = formatter.ReadPackable<ElevatorPlatformPassenger[]>(state);
				break;
			case 23:
				_cutscene = formatter.ReadPackable<EntityRef<CutscenePlayerData>>(state);
				break;
			case 24:
				Unsafe.AsRef(in CutsceneHold) = formatter.ReadPackable<CountableFlag>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
