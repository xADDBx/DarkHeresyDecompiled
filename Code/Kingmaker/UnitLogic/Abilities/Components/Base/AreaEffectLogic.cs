using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAreaEffect))]
[AllowedOn(typeof(BlueprintAreaEffectClusterLogic))]
[TypeId("2ca53113ae97252469a3609d4d73f686")]
public abstract class AreaEffectLogic : BlueprintComponent
{
	[Flags]
	private enum Options
	{
		None = 0,
		DoNotTreatEnterAsMovement = 1
	}

	[SerializeField]
	private Options m_Options;

	public void HandleEntityEnter(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityEnter(context, areaEffect, entity);
		EventBus.RaiseEvent((IAreaEffectEntity)areaEffect, (Action<IAreaEffectEnterHandler>)delegate(IAreaEffectEnterHandler h)
		{
			h.HandleUnitEnterAreaEffect(entity);
		}, isCheckRuntime: true);
		if ((m_Options & Options.DoNotTreatEnterAsMovement) == 0)
		{
			HandleEntityMove(context, areaEffect, entity);
		}
	}

	public void HandleEntityTurnStart(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityTurnStart(context, areaEffect, entity);
	}

	public void HandleEntityTurnEnd(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityTurnEnd(context, areaEffect, entity);
	}

	public void HandleEntityExit(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityExit(context, areaEffect, entity);
	}

	public void HandleEntityMove(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityMove(context, areaEffect, entity);
	}

	public void HandleRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnRound(context, areaEffect);
	}

	public void HandleTick(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnTick(context, areaEffect);
	}

	public void HandleEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnEndForEachEntity(context, areaEffect);
		OnEnd(context, areaEffect);
	}

	protected virtual void OnEntityEnter(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnEntityExit(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnEntityMove(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnTick(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEndForEachEntity(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEntityTurnStart(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnEntityTurnEnd(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}
}
