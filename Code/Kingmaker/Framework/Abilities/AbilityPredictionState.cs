using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;

namespace Kingmaker.Framework.Abilities;

public class AbilityPredictionState : IDisposable
{
	private readonly Dictionary<MechanicEntity, MechanicEntity> _copies = new Dictionary<MechanicEntity, MechanicEntity>();

	private readonly Dictionary<MechanicEntity, MechanicEntity> _originals = new Dictionary<MechanicEntity, MechanicEntity>();

	private readonly EventBusInstance _eventBus = new EventBusInstance();

	public MechanicEntity GetPredictionEntity(MechanicEntity entity)
	{
		if (_originals.ContainsKey(entity))
		{
			return entity;
		}
		if (_copies.TryGetValue(entity, out MechanicEntity value))
		{
			return value;
		}
		MechanicEntity mechanicEntity;
		if (!(entity is BaseUnitEntity unit))
		{
			if (!(entity is DestructibleEntity original))
			{
				throw new ArgumentException("Cannot create prediction copy of " + entity.GetType().Name + " — only units and destructibles are supported");
			}
			mechanicEntity = CopyDestructible(original);
		}
		else
		{
			mechanicEntity = CopyUnit(unit);
		}
		MechanicEntity mechanicEntity2 = mechanicEntity;
		_copies[entity] = mechanicEntity2;
		_originals[mechanicEntity2] = entity;
		return mechanicEntity2;
	}

	public MechanicEntity GetOriginalEntity(MechanicEntity entity)
	{
		return _originals.GetValueOrDefault(entity, entity);
	}

	public bool IsPredictionCopy(MechanicEntity entity)
	{
		return _originals.ContainsKey(entity);
	}

	public void Dispose()
	{
		foreach (MechanicEntity value in _copies.Values)
		{
			value.Dispose();
		}
		_copies.Clear();
		_originals.Clear();
	}

	private MechanicEntity CopyUnit(BaseUnitEntity unit)
	{
		return unit.Copy(createView: false, preview: true, copyItems: true, copyBuffs: true, _eventBus);
	}

	private MechanicEntity CopyDestructible(DestructibleEntity original)
	{
		DestructibleEntity destructibleEntity = new DestructibleEntity(original.Config);
		destructibleEntity.Health.SetHitPointsLeft(original.Health.HitPointsLeft);
		return destructibleEntity;
	}
}
