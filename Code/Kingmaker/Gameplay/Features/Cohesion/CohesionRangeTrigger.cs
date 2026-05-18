using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("91ee3366021a4dac9dbc4583e2e766b0")]
public abstract class CohesionRangeTrigger : UnitFactComponentDelegate
{
	protected enum EventType
	{
		Enter,
		Exit,
		Move,
		StartTurn,
		EndTurn
	}

	[Tooltip("CurrentEntity - с кем произошло событие (кто-то вошел в Cohesion Range владельца или в чей Cohesion Range зашел владелец, в зависимости от типа триггера)")]
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public TargetType Filter;

	[InfoBox("Юнит вошел в кохижен")]
	public ActionList OnEnter;

	[InfoBox("Юнит вышел из кохижена")]
	public ActionList OnExit;

	[InfoBox("Юнит переместился в кохижене")]
	public ActionList OnMove;

	[InfoBox("Юнит начал ход в кохижене")]
	public ActionList OnStartTurn;

	[InfoBox("Юнит закончил ход в кохижене")]
	public ActionList OnEndTurn;

	protected void TryTrigger(EventType eventType, MechanicEntity entity)
	{
		if (IsSuitable(entity))
		{
			(eventType switch
			{
				EventType.Enter => OnEnter, 
				EventType.Exit => OnExit, 
				EventType.Move => OnMove, 
				EventType.StartTurn => OnStartTurn, 
				EventType.EndTurn => OnEndTurn, 
				_ => throw new ArgumentOutOfRangeException("eventType", eventType, null), 
			}).RunWithTarget(entity);
		}
	}

	private bool IsSuitable(MechanicEntity entity)
	{
		if (Filter switch
		{
			TargetType.Enemy => base.Owner.IsEnemy(entity), 
			TargetType.Ally => base.Owner.IsAlly(entity), 
			TargetType.Any => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		})
		{
			return Restriction.IsPassed(base.Context, entity);
		}
		return false;
	}
}
