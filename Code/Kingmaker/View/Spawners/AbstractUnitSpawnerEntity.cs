using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractUnitSpawnerEntity : SimpleEntity, IHashable, IOwlPackable<AbstractUnitSpawnerEntity>
{
	[JsonProperty]
	[OwlPackInclude]
	protected EntityRef<AbstractUnitEntity> m_SpawnedUnit;

	[JsonProperty]
	[OwlPackInclude]
	public bool HasSpawned { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[OwlPackInclude]
	public bool HasDied { get; set; }

	public new IAbstractUnitSpawnerConfig Config => (IAbstractUnitSpawnerConfig)base.Config;

	public BlueprintUnit Blueprint => Config.Blueprint;

	[CanBeNull]
	public AbstractUnitEntity SpawnedUnit
	{
		get
		{
			return m_SpawnedUnit;
		}
		protected set
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

	public bool SpawnedUnitHasDied
	{
		get
		{
			if (HasSpawned)
			{
				return SpawnedUnit?.LifeState.IsDead ?? HasDied;
			}
			return false;
		}
	}

	public override bool IsViewActive => true;

	protected AbstractUnitSpawnerEntity(IAbstractUnitSpawnerConfig config)
		: base(config)
	{
	}

	protected AbstractUnitSpawnerEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public virtual bool ShouldProcessActivation(bool alsoRaiseDead)
	{
		IAbstractUnitSpawnerConfig config = Config;
		if (config == null)
		{
			return false;
		}
		if (config.SpawnOnSceneInit)
		{
			if (HasSpawned && (!alsoRaiseDead || !config.RespawnIfDead || !SpawnedUnitHasDied))
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
		AbstractUnitEntity spawnedUnit = SpawnedUnit;
		if (spawnedUnit != null)
		{
			ApplyOnSpawn(spawnedUnit);
			HasSpawned = true;
		}
	}

	private void ApplyOnSpawn(AbstractUnitEntity unit)
	{
		unit.SetGroup(new EntityRef<UnitGroupEntity>(Config.GroupId));
		Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
		{
			d.OnSpawn(unit);
		});
		ApplyOnInitialize(unit);
	}

	private void ApplyOnInitialize(AbstractUnitEntity unit)
	{
		unit.SetGroup(new EntityRef<UnitGroupEntity>(Config.GroupId));
		Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
		{
			d.OnInitialize(unit);
		});
	}

	protected override void OnSetConfig(IEntityConfig config)
	{
		base.OnSetConfig(config);
		AbstractUnitEntity spawnedUnit = SpawnedUnit;
		if (spawnedUnit != null)
		{
			ApplyOnInitialize(spawnedUnit);
			if ((spawnedUnit.SpawnPosition.To2D() - spawnedUnit.Position.To2D()).magnitude < 0.1f)
			{
				Vector3 spawnPosition = (spawnedUnit.Position = Config.Position);
				spawnedUnit.SpawnPosition = spawnPosition;
				spawnedUnit.SetOrientation(Config.Orientation);
			}
		}
	}

	private void ApplyOnDispose()
	{
		AbstractUnitEntity unit = SpawnedUnit;
		if (unit != null && !unit.WillBeDestroyed && !unit.Destroyed)
		{
			Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
			{
				d.OnDispose(unit);
			});
		}
	}

	protected override IEntityView CreateViewForData()
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

	public virtual void HandleAreaSpawnerInit()
	{
		if ((!HasSpawned || (SpawnedUnitHasDied && Config.RespawnIfDead)) && Config.SpawnOnSceneInit && CheckConditions())
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(SpawnedUnit);
			Clear();
			Spawn();
		}
		else if (HasSpawned && Game.Instance.Player.BrokenEntities.Contains(m_SpawnedUnit.Id))
		{
			PFLog.Entity.Error("Respawning broken unit! {0}", m_SpawnedUnit.Id);
			Game.Instance.Player.BrokenEntities.Remove(m_SpawnedUnit.Id);
			ForceReSpawn();
		}
	}

	private bool CheckConditions()
	{
		if (Config.SpawnConditions != null)
		{
			ConditionsChecker conditions = Config.SpawnConditions.Conditions;
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
			PFLog.Entity.Warning("Trying to use spawner {0} twice.", Config.name);
			return null;
		}
		if (Config.Blueprint == null)
		{
			PFLog.Entity.ErrorWithReport("UnitSpawnerBase.Spawn: unit blueprint is null! " + Config.name);
			return null;
		}
		UnitSpawnRestrictionResult unitSpawnRestrictionResult = UnitSpawnRestrictionResult.CanSpawn;
		IUnitSpawnRestriction[] restrictions = Config.Restrictions;
		for (int i = 0; i < restrictions.Length; i++)
		{
			UnitSpawnRestrictionResult unitSpawnRestrictionResult2 = restrictions[i].CanSpawn();
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
			AbstractUnitEntity abstractUnitEntity = SpawnUnit(Config.Position, Config.Rotation);
			if (abstractUnitEntity == null)
			{
				return null;
			}
			SpawnedUnit = abstractUnitEntity;
			abstractUnitEntity.SpawnPosition = Config.Position;
			abstractUnitEntity.View.ForcePlaceAboveGround();
			return abstractUnitEntity;
		}
		}
	}

	public void ForceReSpawn()
	{
		Game.Instance.Controllers.EntityDestroyer.Destroy(SpawnedUnit);
		Clear();
		AbstractUnitEntity abstractUnitEntity = SpawnUnit(Config.Position, Config.Rotation);
		if (abstractUnitEntity != null)
		{
			SpawnedUnit = abstractUnitEntity;
			abstractUnitEntity.SpawnPosition = Config.Position;
			abstractUnitEntity.View.ForcePlaceAboveGround();
		}
	}

	protected virtual AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		return null;
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
}
