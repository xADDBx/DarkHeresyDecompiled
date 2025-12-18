using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("dfea570039374dd99e2e6cf487f9add8")]
public class CompanionSpawner : UnitSpawnerBase, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAddInspectorGUI, ICompanionChangeHandler, IEtudesUpdateHandler
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

	[OwlPackable(OwlPackableMode.Generate)]
	public new class MyData : UnitSpawnerBase.MyData, IOwlPackable<MyData>
	{
		private bool m_NeedInitOnPlace;

		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "MyData",
			OldNames = null,
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

		public MyData(EntityViewBase view)
			: base(view)
		{
		}

		private MyData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected MyData()
		{
		}

		public override bool ShouldProcessActivation(bool alsoRaiseDead)
		{
			return true;
		}

		public void OnPlaceCompanion(bool didPlace)
		{
			AbstractUnitEntity unit = base.SpawnedUnit.Entity;
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

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			MyData source = new MyData();
			result = Unsafe.As<MyData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<MyData>(OwlPackTypeInfo);
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
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MyData>();
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

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenRemote;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenInCapital;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenDetached;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenEx = true;

	[SerializeField]
	[ShowCreator]
	[HideIf("m_SpawnNpcCopy")]
	private ConditionsReference ControlCondition;

	[SerializeField]
	[ShowCreator]
	[HideIf("m_SpawnNpcCopy")]
	private ConditionsReference ShowCondition;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private BlueprintFactionReference m_OverrideFaction;

	[SerializeField]
	[AddInspector]
	[UsedImplicitly]
	private bool m_Dummy;

	[SerializeField]
	[Tooltip("Spawn an NPC companion copy instead, that have exactly the same look")]
	private bool m_SpawnNpcCopy;

	[NonSerialized]
	private LogContext spawnLogContext;

	private new MyData Data => (MyData)base.Data;

	private new BaseUnitEntity SpawnedUnit => (BaseUnitEntity)base.SpawnedUnit;

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

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MyData(this));
	}

	private BaseUnitEntity GetMyCompanion()
	{
		return Game.Instance.Player.AllCharacters.SingleItem((BaseUnitEntity u) => u.Blueprint.CheckEqualsWithPrototype(base.Blueprint));
	}

	public override void HandleAreaSpawnerInit()
	{
		spawnLogContext = LogContext.SceneInit;
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
		if (myCompanion == null)
		{
			LogSpawn("Companion is not recruited yet.");
			return null;
		}
		if (m_SpawnNpcCopy)
		{
			return SpawnNpcCopy(myCompanion);
		}
		ClaimCompanion();
		return myCompanion;
	}

	private AbstractUnitEntity SpawnNpcCopy(BaseUnitEntity companion)
	{
		BaseUnitEntity baseUnitEntity = SummonUnitCopy.CreateCopy(companion, base.Blueprint, Data.HoldingState, doNotCreateItems: true);
		if (baseUnitEntity == null)
		{
			LogSpawn("Failed to spawn companion NPC copy.");
			return null;
		}
		Vector3 position2 = (baseUnitEntity.Position = base.ViewTransform.position);
		baseUnitEntity.SetOrientation(base.ViewTransform.rotation.eulerAngles.y);
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
		else if (Data == null || (!Data.DidSpawnOnce && !base.SpawnOnSceneInit))
		{
			msg = "Spawn was not triggered yet.  Just checking..";
			flag = false;
		}
		else
		{
			CompanionSpawner companionSpawner = unit.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner();
			if (companionSpawner != null && companionSpawner != this && companionSpawner.ShouldControlUnit(unit))
			{
				msg = "Already controlled by spawner '" + companionSpawner.name + "'";
				flag = false;
			}
			else
			{
				CompanionState companionState = unit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
				bool capitalPartyMode = Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
				if ((!m_SpawnWhenRemote || companionState != CompanionState.Remote) && (!m_SpawnWhenDetached || companionState != CompanionState.InPartyDetached) && (!m_SpawnWhenEx || companionState != CompanionState.ExCompanion) && (!(m_SpawnWhenInCapital && capitalPartyMode) || (companionState != CompanionState.Remote && companionState != CompanionState.InParty)))
				{
					msg = $"State is not a match. Companion state:{companionState}";
					flag = false;
				}
				else
				{
					ConditionsHolder conditionsHolder = ShowCondition?.Get();
					if (conditionsHolder != null && !conditionsHolder.Check())
					{
						msg = "Show condition is false.";
						flag = false;
					}
					else
					{
						ConditionsHolder conditionsHolder2 = ControlCondition?.Get();
						if (conditionsHolder2 != null && !conditionsHolder2.Check())
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
		Data.SpawnedUnit = myCompanion;
		BlueprintFaction blueprintFaction = m_OverrideFaction?.Get();
		if (blueprintFaction != null)
		{
			myCompanion.Faction.Set(blueprintFaction);
		}
		PlaceCompanion(myCompanion, stayInGame);
	}

	public void ReleaseCompanion()
	{
		LogSpawn("Companion released.");
		Data.Clear();
	}

	private void UpdateState(bool stayInGame = false)
	{
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
			unit.Position = base.ViewTransform.position;
			unit.SetOrientation(base.ViewTransform.rotation.eulerAngles.y);
			if ((bool)unit.View)
			{
				unit.View.ViewTransform.position = base.ViewTransform.position;
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
		LogSpawn(flag ? "Spawn Success!" : "Spawn failed.");
		Data.OnPlaceCompanion(flag);
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
}
