using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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

	public void HandleEntityEnter(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
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

	public void HandleEntityTurnStart(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityTurnStart(context, areaEffect, entity);
	}

	public void HandleEntityTurnEnd(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityTurnEnd(context, areaEffect, entity);
	}

	public void HandleEntityExit(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityExit(context, areaEffect, entity);
	}

	public void HandleEntityMove(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		OnEntityMove(context, areaEffect, entity);
	}

	public void HandleRound(IEvalContext context, AreaEffectEntity areaEffect)
	{
		OnRound(context, areaEffect);
	}

	public void HandleTick(IEvalContext context, AreaEffectEntity areaEffect)
	{
		OnTick(context, areaEffect);
	}

	public void HandleEnd(IEvalContext context, AreaEffectEntity areaEffect)
	{
		OnEndForEachEntity(context, areaEffect);
		OnEnd(context, areaEffect);
	}

	protected virtual void OnEntityEnter(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnEntityExit(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnEntityMove(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnRound(IEvalContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnTick(IEvalContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEndForEachEntity(IEvalContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEnd(IEvalContext context, AreaEffectEntity areaEffect)
	{
	}

	protected virtual void OnEntityTurnStart(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}

	protected virtual void OnEntityTurnEnd(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
	}
}
