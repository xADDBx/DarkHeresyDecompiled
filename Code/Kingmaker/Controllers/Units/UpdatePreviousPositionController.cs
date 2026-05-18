using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UpdatePreviousPositionController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (AbstractUnitEntity item in default(MovableEntitiesEnumerable))
		{
			try
			{
				ProcessUnit(item);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		static void ProcessUnit(AbstractUnitEntity unit)
		{
			AbstractUnitEntityView abstractUnitEntityView = unit.View.AsAbstractUnitEntityView();
			if ((object)abstractUnitEntityView != null)
			{
				bool forceUpdatePositions = (bool)unit.Features.OnElevator || unit.GetOptional<EntityPartStayOnPlatform>() != null;
				abstractUnitEntityView.InterpolationHelper.OnUnitSimulationTickCompleted(forceUpdatePositions);
				unit.Movable.PreviousSimulationTick = new PartMovable.PreviousSimulationTickInfo
				{
					HasMotion = (unit.Movable.HasMotionThisSimulationTick || unit.Movable.ForceHasMotion),
					HasRotation = !Mathf.Approximately(unit.Orientation, unit.Movable.PreviousOrientation)
				};
				unit.Movable.ForceHasMotion = false;
				unit.Movable.PreviousPosition = unit.Position;
				unit.Movable.PreviousOrientation = unit.Orientation;
			}
		}
	}
}
