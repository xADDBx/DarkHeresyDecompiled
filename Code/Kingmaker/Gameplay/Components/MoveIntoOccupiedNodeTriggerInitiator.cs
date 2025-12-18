using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Mechanics;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Pathfinding;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("Triggers when unit enter node which is occupied by someone else. Works only in combat.")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("43b86b37d7854e91acd9813cfa28b5bb")]
public sealed class MoveIntoOccupiedNodeTriggerInitiator : UnitFactComponentDelegate, IUnitNodeChangedHandler<EntitySubscriber>, IUnitNodeChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitNodeChangedHandler, EntitySubscriber>, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IEventTag<IUnitCommandEndHandler, EntitySubscriber>
{
	private sealed class ComponentData : IEntityFactComponentTransientData
	{
		public readonly HashSet<BaseUnitEntity> UsedUnits = new HashSet<BaseUnitEntity>();
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList Actions = new ActionList();

	void IUnitNodeChangedHandler.HandleUnitNodeChanged(GraphNode oldNode)
	{
		if (!Game.Instance.Controllers.TurnController.InCombat)
		{
			return;
		}
		HashSet<BaseUnitEntity> usedUnits = RequestTransientData<ComponentData>().UsedUnits;
		foreach (GridNodeBase occupiedNode in base.Owner.GetOccupiedNodes())
		{
			foreach (BaseUnitEntity unit in occupiedNode.GetUnits())
			{
				if (unit != base.Owner && usedUnits.Add(unit) && !unit.IsDead && Restrictions.IsPassed(base.Context, unit))
				{
					Actions.RunWithTarget(unit);
				}
			}
		}
	}

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		RequestTransientData<ComponentData>().UsedUnits.Clear();
	}
}
