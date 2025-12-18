using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("6011d470489d44f18bb1b158e71ade47")]
public abstract class UnitSpawnerBase : EntityViewBase
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class MyData : SimpleEntity, IHashable, IOwlPackable<MyData>
	{
		[JsonProperty]
		[OwlPackInclude]
		protected EntityRef<AbstractUnitEntity> m_SpawnedUnit;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "MyData",
			OldNames = null,
			Fields = new FieldInfo[13]
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
				new FieldInfo("m_SpawnedUnit", typeof(EntityRef<AbstractUnitEntity>))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public bool HasSpawned { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		[OwlPackInclude]
		public bool HasDied { get; set; }

		public new UnitSpawnerBase View => (UnitSpawnerBase)base.View;

		public EntityRef<AbstractUnitEntity> SpawnedUnit
		{
			get
			{
				return m_SpawnedUnit;
			}
			set
			{
				if (m_SpawnedUnit != value)
				{
					if (m_SpawnedUnit != null)
					{
						Clear();
					}
					m_SpawnedUnit = value;
					OnSpawned();
				}
			}
		}

		public override bool IsViewActive => true;

		public MyData(EntityViewBase view)
			: base(view)
		{
		}

		protected MyData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected MyData()
		{
		}

		public virtual bool ShouldProcessActivation(bool alsoRaiseDead)
		{
			UnitSpawnerBase view = View;
			if (!view)
			{
				return false;
			}
			if (view.SpawnOnSceneInit)
			{
				if (view.HasSpawned && (!alsoRaiseDead || !view.m_RespawnIfDead || !view.SpawnedUnitHasDied))
				{
					if (HasSpawned)
					{
						return Game.Instance.Player.BrokenEntities.Contains(m_SpawnedUnit.Id);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		protected virtual void OnSpawned()
		{
			AbstractUnitEntity entity = SpawnedUnit.Entity;
			if (entity != null)
			{
				ApplyOnSpawn(entity);
				HasSpawned = true;
			}
		}

		private void ApplyOnSpawn(AbstractUnitEntity unit)
		{
			unit.SetGroup(new EntityRef<UnitGroupView.UnitGroupData>(View.GroupId));
			Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
			{
				d.OnSpawn(unit);
			});
			ApplyOnInitialize(unit);
		}

		private void ApplyOnInitialize(AbstractUnitEntity unit)
		{
			unit.SetGroup(new EntityRef<UnitGroupView.UnitGroupData>(View.GroupId));
			try
			{
				View?.OnInitialize(unit);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
			Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
			{
				d.OnInitialize(unit);
			});
		}

		private void ApplyOnDispose()
		{
			AbstractUnitEntity unit = SpawnedUnit.Entity;
			if (unit != null && !unit.WillBeDestroyed && !unit.Destroyed)
			{
				Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
				{
					d.OnDispose(unit);
				});
			}
		}

		protected override void OnViewDidAttach()
		{
			base.OnViewDidAttach();
			AbstractUnitEntity entity = SpawnedUnit.Entity;
			if (entity != null)
			{
				ApplyOnInitialize(entity);
				if ((entity.SpawnPosition.To2D() - entity.Position.To2D()).magnitude < 0.1f)
				{
					Vector3 spawnPosition = (entity.Position = View.ViewTransform.position);
					entity.SpawnPosition = spawnPosition;
					entity.SetOrientation(View.ViewTransform.rotation.eulerAngles.y);
				}
			}
		}

		protected override IEntityViewBase CreateViewForData()
		{
			return null;
		}

		protected override void OnDispose()
		{
			Clear();
			base.OnDispose();
		}

		public void Clear()
		{
			ApplyOnDispose();
			m_SpawnedUnit = default(EntityRef<AbstractUnitEntity>);
			HasSpawned = false;
		}

		protected override void OnIsInGameChanged()
		{
			base.OnIsInGameChanged();
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			bool val2 = HasSpawned;
			result.Append(ref val2);
			bool val3 = HasDied;
			result.Append(ref val3);
			EntityRef<AbstractUnitEntity> obj = m_SpawnedUnit;
			Hash128 val4 = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj);
			result.Append(ref val4);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
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
			bool value2 = HasSpawned;
			formatter.UnmanagedField(10, "HasSpawned", ref value2, state);
			bool value3 = HasDied;
			formatter.UnmanagedField(11, "HasDied", ref value3, state);
			formatter.Field(12, "m_SpawnedUnit", ref m_SpawnedUnit, state);
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
					HasSpawned = formatter.ReadUnmanaged<bool>(state);
					break;
				case 11:
					HasDied = formatter.ReadUnmanaged<bool>(state);
					break;
				case 12:
					m_SpawnedUnit = formatter.ReadPackable<EntityRef<AbstractUnitEntity>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Blueprint;

	[SerializeField]
	private bool m_SpawnOnSceneInit = true;

	[SerializeField]
	private bool m_RespawnIfDead;

	[SerializeField]
	[ShowCreator]
	private ConditionsReference m_spawnConditions;

	[HideInInspector]
	[SerializeField]
	private UnitCustomizationVariation m_SelectedCustomizationVariation;

	[CanBeNull]
	private UnitGroupView m_Group;

	public bool HasSpawned
	{
		get
		{
			return Data.HasSpawned;
		}
		protected set
		{
			Data.HasSpawned = value;
		}
	}

	public BlueprintUnit Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	public AbstractUnitEntity SpawnedUnit
	{
		get
		{
			EntityRef<AbstractUnitEntity>? entityRef = Data?.SpawnedUnit;
			if (!entityRef.HasValue)
			{
				return null;
			}
			return entityRef.GetValueOrDefault();
		}
	}

	public bool SpawnOnSceneInit => m_SpawnOnSceneInit;

	public bool SpawnedUnitHasDied
	{
		get
		{
			if (HasSpawned)
			{
				return SpawnedUnit?.LifeState.IsDead ?? Data.HasDied;
			}
			return false;
		}
	}

	public new MyData Data => (MyData)base.Data;

	public override bool CreatesDataOnLoad => true;

	public bool HasCustomizationPreset => Blueprint?.CustomizationPreset != null;

	[CanBeNull]
	public string GroupId => m_Group?.UniqueId;

	public UnitCustomizationVariation SelectedCustomizationVariation => m_SelectedCustomizationVariation;

	protected override void Awake()
	{
		base.Awake();
		m_Group = GetComponentInParent<UnitGroupView>();
	}

	public virtual void HandleAreaSpawnerInit()
	{
		if ((!HasSpawned || (SpawnedUnitHasDied && m_RespawnIfDead)) && m_SpawnOnSceneInit && CheckConditions())
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(Data.SpawnedUnit.Entity);
			Data.Clear();
			Spawn();
		}
		else if (HasSpawned && Game.Instance.Player.BrokenEntities.Contains(Data.SpawnedUnit.Id))
		{
			UberDebug.LogError("Respawning broken unit! {0}", Data.SpawnedUnit.Id);
			Game.Instance.Player.BrokenEntities.Remove(Data.SpawnedUnit.Id);
			ForceReSpawn();
		}
	}

	private bool CheckConditions()
	{
		if (m_spawnConditions.Get() != null)
		{
			ConditionsChecker conditions = m_spawnConditions.Get().Conditions;
			if (conditions.HasConditions)
			{
				return conditions.Check();
			}
			return true;
		}
		return true;
	}

	[CanBeNull]
	public AbstractUnitEntity Spawn()
	{
		if (HasSpawned)
		{
			PFLog.Default.Warning("Trying to use spawner {0} twice.", base.name);
			return null;
		}
		if (m_Blueprint.IsEmpty())
		{
			PFLog.Default.ErrorWithReport("UnitSpawnerBase.Spawn: unit blueprint is null! " + base.name);
			return null;
		}
		List<IUnitSpawnRestriction> list = TempList.Get<IUnitSpawnRestriction>();
		GetComponents(list);
		UnitSpawnRestrictionResult unitSpawnRestrictionResult = UnitSpawnRestrictionResult.CanSpawn;
		foreach (IUnitSpawnRestriction item in list)
		{
			UnitSpawnRestrictionResult unitSpawnRestrictionResult2 = item.CanSpawn();
			if (unitSpawnRestrictionResult2 > unitSpawnRestrictionResult)
			{
				unitSpawnRestrictionResult = unitSpawnRestrictionResult2;
			}
		}
		switch (unitSpawnRestrictionResult)
		{
		case UnitSpawnRestrictionResult.Delay:
			return null;
		case UnitSpawnRestrictionResult.Disable:
			HasSpawned = true;
			return null;
		default:
		{
			AbstractUnitEntity abstractUnitEntity = SpawnUnit(base.ViewTransform.position, base.ViewTransform.rotation);
			if (abstractUnitEntity == null)
			{
				return null;
			}
			Data.SpawnedUnit = abstractUnitEntity;
			abstractUnitEntity.SpawnPosition = base.ViewTransform.position;
			abstractUnitEntity.View.ForcePlaceAboveGround();
			return abstractUnitEntity;
		}
		}
	}

	public void ForceReSpawn()
	{
		Game.Instance.Controllers.EntityDestroyer.Destroy(Data.SpawnedUnit.Entity);
		Data.Clear();
		AbstractUnitEntity abstractUnitEntity = SpawnUnit(base.transform.position, base.transform.rotation);
		if (abstractUnitEntity != null)
		{
			Data.SpawnedUnit = abstractUnitEntity;
			abstractUnitEntity.SpawnPosition = base.transform.position;
			abstractUnitEntity.View.ForcePlaceAboveGround();
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MyData(this));
	}

	protected virtual AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		return null;
	}

	protected virtual void OnInitialize(AbstractUnitEntity unit)
	{
	}
}
