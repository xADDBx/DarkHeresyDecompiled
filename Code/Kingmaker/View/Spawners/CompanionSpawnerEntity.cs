using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.View.Spawners.CompanionSpawner+MyData")]
public sealed class CompanionSpawnerEntity : AbstractUnitSpawnerEntity, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ICompanionChangeHandler, IEtudesUpdateHandler, IHashable, IOwlPackable<CompanionSpawnerEntity>
{
	private enum LogContext
	{
		Unknown,
		SceneInit,
		SpawnUnit,
		AddedToParty,
		Activated,
		RemovedFromParty,
		CapitalModeChanged,
		Recruited,
		Unrecruited,
		EtudeUpdate
	}

	private bool m_NeedInitOnPlace;

	[NonSerialized]
	private LogContext spawnLogContext;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CompanionSpawnerEntity",
		OldNames = new string[1] { "Kingmaker.View.Spawners.CompanionSpawner+MyData" },
		Fields = new FieldInfo[14]
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
			new FieldInfo("HasSpawned", typeof(bool)),
			new FieldInfo("HasDied", typeof(bool)),
			new FieldInfo("m_SpawnedUnit", typeof(EntityRef<AbstractUnitEntity>)),
			new FieldInfo("DidSpawnOnce", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool DidSpawnOnce { get; private set; }

	public new ICompanionSpawnerConfig Config => (ICompanionSpawnerConfig)base.Config;

	public bool IsControllingCompanion
	{
		get
		{
			BaseUnitEntity myCompanion = GetMyCompanion();
			if (myCompanion != null && myCompanion == SpawnedUnit)
			{
				return myCompanion.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner() == this;
			}
			return false;
		}
	}

	public new BaseUnitEntity SpawnedUnit
	{
		get
		{
			return (BaseUnitEntity)base.SpawnedUnit;
		}
		private set
		{
			base.SpawnedUnit = value;
		}
	}

	public CompanionSpawnerEntity(ICompanionSpawnerConfig config)
		: base(config)
	{
	}

	private CompanionSpawnerEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public override bool ShouldProcessActivation(bool alsoRaiseDead)
	{
		return true;
	}

	public void OnPlaceCompanion(bool didPlace)
	{
		BaseUnitEntity unit = SpawnedUnit;
		if (unit == null || unit.WillBeDestroyed || unit.Destroyed)
		{
			return;
		}
		if (!didPlace)
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional != null && optional.IsControllableInParty() && !m_NeedInitOnPlace)
			{
				Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
				{
					d.OnDispose(unit);
				});
				m_NeedInitOnPlace = true;
			}
		}
		if (didPlace && m_NeedInitOnPlace)
		{
			Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
			{
				d.OnInitialize(unit);
			});
			m_NeedInitOnPlace = false;
		}
	}

	protected override void OnSpawned()
	{
		base.OnSpawned();
		m_NeedInitOnPlace = false;
		DidSpawnOnce = true;
	}

	private BaseUnitEntity GetMyCompanion()
	{
		return Game.Instance.Player.AllCharacters.SingleItem((BaseUnitEntity u) => u.Blueprint.CheckEqualsWithPrototype(Config.Blueprint));
	}

	public override void HandleAreaSpawnerInit()
	{
		spawnLogContext = LogContext.SceneInit;
		if (!Config.SpawnOnSceneInit || Config.SpawnNpcCopy)
		{
			base.HandleAreaSpawnerInit();
			return;
		}
		bool flag = ShouldControlUnit(GetMyCompanion());
		if (base.HasSpawned)
		{
			if (IsControllingCompanion && flag)
			{
				PlaceCompanion(SpawnedUnit, stayInGame: false);
			}
			else
			{
				ReleaseCompanion();
			}
		}
		if (flag)
		{
			base.HandleAreaSpawnerInit();
		}
	}

	protected override AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		spawnLogContext = LogContext.SpawnUnit;
		BaseUnitEntity myCompanion = GetMyCompanion();
		if (Config.SpawnNpcCopy)
		{
			return SpawnNpcCopy(myCompanion);
		}
		if (myCompanion == null)
		{
			LogSpawn("Companion is not recruited yet.");
			return null;
		}
		ClaimCompanion();
		return myCompanion;
	}

	private AbstractUnitEntity SpawnNpcCopy(BaseUnitEntity companion)
	{
		BaseUnitEntity baseUnitEntity = ((companion == null) ? Game.Instance.Controllers.EntitySpawner.SpawnUnit(base.Blueprint, Vector3.zero, Quaternion.identity, HoldingState, Config.SelectedCustomizationVariation) : SummonUnitCopy.CreateCopy(companion, base.Blueprint, HoldingState, doNotCreateItems: true));
		if (baseUnitEntity == null)
		{
			LogSpawn("Failed to spawn companion NPC copy.");
			return null;
		}
		Vector3 position2 = (baseUnitEntity.Position = Config.Position);
		baseUnitEntity.SetOrientation(Config.Rotation.eulerAngles.y);
		baseUnitEntity.View.ViewTransform.position = position2;
		GameObject gameObject = baseUnitEntity.View.gameObject;
		gameObject.name = "NPC_COPY_" + gameObject.name;
		LogSpawn("NPC copy spawned: " + gameObject.name);
		return baseUnitEntity;
	}

	private bool ShouldControlUnit(BaseUnitEntity unit)
	{
		bool flag = true;
		string msg = string.Empty;
		if (unit == null)
		{
			msg = "Companion is not recruited yet.";
			flag = false;
		}
		else if (!DidSpawnOnce && !Config.SpawnOnSceneInit)
		{
			msg = "Spawn was not triggered yet.  Just checking..";
			flag = false;
		}
		else
		{
			CompanionSpawnerEntity companionSpawnerEntity = unit.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner();
			if (companionSpawnerEntity != null && companionSpawnerEntity != this && companionSpawnerEntity.ShouldControlUnit(unit))
			{
				msg = "Already controlled by spawner '" + companionSpawnerEntity.Config.ViewName + "'";
				flag = false;
			}
			else
			{
				CompanionState companionState = unit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
				bool capitalPartyMode = Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
				if ((!Config.SpawnWhenRemote || companionState != CompanionState.Remote) && (!Config.SpawnWhenDetached || companionState != CompanionState.InPartyDetached) && (!Config.SpawnWhenEx || companionState != CompanionState.ExCompanion) && (!(Config.SpawnWhenInCapital && capitalPartyMode) || (companionState != CompanionState.Remote && companionState != CompanionState.InParty)))
				{
					msg = $"State is not a match. Companion state:{companionState}";
					flag = false;
				}
				else
				{
					ConditionsHolder showCondition = Config.ShowCondition;
					if (showCondition != null && !showCondition.Check())
					{
						msg = "Show condition is false.";
						flag = false;
					}
					else
					{
						ConditionsHolder controlCondition = Config.ControlCondition;
						if (controlCondition != null && !controlCondition.Check())
						{
							msg = "Control condition is false.";
							flag = false;
						}
					}
				}
			}
		}
		if (!flag)
		{
			LogSpawn("Spawner failed to get companion control. Reason:");
			LogSpawn(msg);
		}
		return flag;
	}

	private void ClaimCompanion(bool stayInGame = false)
	{
		BaseUnitEntity myCompanion = GetMyCompanion();
		myCompanion.GetOptional<UnitPartCompanion>()?.SetSpawner(this);
		SpawnedUnit = myCompanion;
		BlueprintFaction overrideFaction = Config.OverrideFaction;
		if (overrideFaction != null)
		{
			myCompanion.Faction.Set(overrideFaction);
		}
		PlaceCompanion(myCompanion, stayInGame);
	}

	public void ReleaseCompanion()
	{
		LogSpawn("Companion released.");
		Clear();
	}

	private void UpdateState(bool stayInGame = false)
	{
		if (Config.SpawnNpcCopy)
		{
			LogSpawn("Spawner is in NPC copy spawn mode.");
			return;
		}
		if (!base.IsInGame)
		{
			LogSpawn("Spawner is deactivated.");
			return;
		}
		BaseUnitEntity myCompanion = GetMyCompanion();
		bool flag = ShouldControlUnit(myCompanion);
		bool isControllingCompanion = IsControllingCompanion;
		if (flag && !isControllingCompanion)
		{
			ClaimCompanion(stayInGame);
		}
		else if (!flag && isControllingCompanion)
		{
			ReleaseCompanion();
		}
	}

	private void PlaceCompanion(BaseUnitEntity unit, bool stayInGame)
	{
		bool flag = false;
		if (ShouldControlUnit(unit))
		{
			unit.IsInGame = true;
			unit.Position = Config.Position;
			unit.SetOrientation(Config.Rotation.eulerAngles.y);
			if (unit.View != null)
			{
				unit.View.ViewTransform.position = Config.Position;
			}
			flag = true;
		}
		else if (!stayInGame)
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional == null || !optional.IsControllableInParty())
			{
				unit.IsInGame = false;
			}
		}
		if (flag && !TryToResurrect(unit))
		{
			flag = false;
			LogSpawn("Failed to resurrect dead companion.");
		}
		LogSpawn(flag ? "Spawn Success!" : "Spawn failed.");
		OnPlaceCompanion(flag);
	}

	private bool TryToResurrect(BaseUnitEntity unit)
	{
		PartLifeState optional = unit.GetOptional<PartLifeState>();
		if (optional == null)
		{
			throw new Exception("No part life state found.");
		}
		if (!optional.ScriptedKill)
		{
			return true;
		}
		if (!(unit.GetOptional<UnitPartCompanion>() ?? throw new Exception("No companion part found for " + unit.Name)).IsExForever)
		{
			LogSpawn("Cannot resurrect companion that is not ex forever.");
			return false;
		}
		optional.Resurrect();
		optional.Health.HealAll();
		IUnitEntityView view = unit.View;
		if (view == null)
		{
			throw new Exception("No view found for " + unit.Name);
		}
		if (view.Animator != null)
		{
			view.Animator.enabled = true;
		}
		UnitAnimationManager animationManager = view.AnimationManager;
		if (animationManager == null)
		{
			throw new Exception("No animation manager found for " + unit.Name);
		}
		animationManager.Disabled = false;
		animationManager.StandUpImmediately();
		return true;
	}

	void IPartyHandler.HandleAddCompanion()
	{
		spawnLogContext = LogContext.AddedToParty;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IPartyHandler.HandleCompanionActivated()
	{
		spawnLogContext = LogContext.Activated;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IPartyHandler.HandleCompanionRemoved(bool stayInGame)
	{
		spawnLogContext = LogContext.RemovedFromParty;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState(stayInGame);
		}
	}

	void IPartyHandler.HandleCapitalModeChanged()
	{
		spawnLogContext = LogContext.CapitalModeChanged;
		UpdateState();
	}

	void ICompanionChangeHandler.HandleRecruit()
	{
		spawnLogContext = LogContext.Recruited;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void ICompanionChangeHandler.HandleUnrecruit()
	{
		spawnLogContext = LogContext.Unrecruited;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IEtudesUpdateHandler.OnEtudesUpdate()
	{
		spawnLogContext = LogContext.EtudeUpdate;
		UpdateState();
	}

	private void LogSpawn(string msg)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = DidSpawnOnce;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CompanionSpawnerEntity source = new CompanionSpawnerEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<CompanionSpawnerEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CompanionSpawnerEntity>(OwlPackTypeInfo);
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
		bool value2 = base.HasSpawned;
		formatter.UnmanagedField(10, "HasSpawned", ref value2, state);
		bool value3 = base.HasDied;
		formatter.UnmanagedField(11, "HasDied", ref value3, state);
		formatter.Field(12, "m_SpawnedUnit", ref m_SpawnedUnit, state);
		bool value4 = DidSpawnOnce;
		formatter.UnmanagedField(13, "DidSpawnOnce", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CompanionSpawnerEntity>();
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
				base.HasSpawned = formatter.ReadUnmanaged<bool>(state);
				break;
			case 11:
				base.HasDied = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_SpawnedUnit = formatter.ReadPackable<EntityRef<AbstractUnitEntity>>(state);
				break;
			case 13:
				DidSpawnOnce = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
