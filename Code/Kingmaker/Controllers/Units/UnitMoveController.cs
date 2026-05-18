using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitMoveController : IControllerEnable, IController, IControllerTick
{
	private class MoveHandlerWrapper
	{
		public AbstractUnitEntity Unit;

		public Action<IUnitMoveHandler> Handler { get; }

		public MoveHandlerWrapper()
		{
			Handler = delegate(IUnitMoveHandler h)
			{
				h.HandleUnitMovement(Unit);
			};
		}
	}

	private static bool s_IsStartRotate;

	private readonly MoveHandlerWrapper m_MoveHandlerWrapper = new MoveHandlerWrapper();

	void IControllerEnable.OnEnable()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			if (allUnit.View != null)
			{
				UnitMovementAgent movementAgent = allUnit.View.MovementAgent;
				if (!(movementAgent == null))
				{
					ObstaclesHelper.ConnectToGroups(movementAgent);
				}
			}
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if (Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && (!AstarPath.active || AstarPath.active.graphs.Length == 0))
		{
			return;
		}
		float gameDeltaTime = Game.Instance.Controllers.TimeController.GameDeltaTime;
		foreach (AbstractUnitEntity item in default(MovableEntitiesEnumerable))
		{
			TickUnit(item, gameDeltaTime);
		}
	}

	private void TickUnit([NotNull] AbstractUnitEntity unit, float dt)
	{
		UnitMovementAgent unitMovementAgent = unit.View?.MovementAgent;
		if (!(unitMovementAgent == null))
		{
			bool isReallyMoving = ApplyChangesByAgent(unit, unitMovementAgent, dt);
			TryUpdateTransformPosition(unit, isReallyMoving);
			TryUpdateTransformOrientation(unit);
		}
	}

	private bool ApplyChangesByAgent(AbstractUnitEntity unit, UnitMovementAgent agent, float dt)
	{
		if ((bool)unit.Features.OnElevator)
		{
			return false;
		}
		bool flag = agent.IsReallyMoving && unit.CanMove;
		bool canRotate = unit.CanRotate;
		if (flag)
		{
			agent.TickMovement(dt);
			unit.Movable.LastMoveTime = Game.Instance.Controllers.TimeController.GameTime;
			if (unit.Movable.HasMotionThisSimulationTick && canRotate && agent.IsReallyMoving && !agent.NodeLinkTraverser.IsTraverseNow)
			{
				Vector3 forward = agent.FaceDirection.To3D();
				if (forward.sqrMagnitude > 0.01f)
				{
					unit.SetOrientation(Quaternion.LookRotation(forward).eulerAngles.y);
				}
			}
			using (ProfileScope.New("Unit move handlers"))
			{
				m_MoveHandlerWrapper.Unit = unit;
				EventBus.RaiseEvent((IAbstractUnitEntity)unit, m_MoveHandlerWrapper.Handler, isCheckRuntime: true);
				m_MoveHandlerWrapper.Unit = null;
			}
		}
		if (canRotate)
		{
			float num = ((Game.Instance.CurrentModeType == GameModeType.Dialog) ? (Game.Instance.Controllers.TimeController.DeltaTime * agent.CurrentAngularSpeed) : (dt * agent.CurrentAngularSpeed * unit.Movable.SlowMoSpeedMod));
			num *= Game.CombatAnimSpeedUp;
			unit.UpdateSlowRotation(num);
		}
		return flag;
	}

	private static void TryUpdateTransformPosition(AbstractUnitEntity unit, bool isReallyMoving)
	{
		AbstractUnitEntityView abstractUnitEntityView = unit.View.AsAbstractUnitEntityView();
		float num = 0f;
		if (unit.Movable.PreviousSimulationTick.HasMotion)
		{
			Vector3 viewPosition = abstractUnitEntityView.InterpolationHelper.GetViewPosition(unit.Movable.PreviousPosition);
			Transform transform = abstractUnitEntityView.transform;
			num = (transform.position - viewPosition).sqrMagnitude;
			transform.position = viewPosition;
		}
		if (!isReallyMoving)
		{
			if (!unit.Features.OnElevator && (unit.Movable.PreviousSimulationTick.HasMotion || (abstractUnitEntityView.AnimationManager != null && abstractUnitEntityView.AnimationManager.IsGoingProne)))
			{
				abstractUnitEntityView.ForcePlaceAboveGround();
			}
			if (num >= 1f)
			{
				abstractUnitEntityView.IkController?.GrounderIk?.ResetPosition();
			}
		}
	}

	private static void TryUpdateTransformOrientation(AbstractUnitEntity unit)
	{
		AbstractUnitEntityView abstractUnitEntityView = unit.View.AsAbstractUnitEntityView();
		if ((Mathf.Approximately(unit.Orientation, unit.Movable.PreviousOrientation) && !unit.Movable.PreviousSimulationTick.HasRotation) || abstractUnitEntityView.ForbidRotation)
		{
			return;
		}
		if (!abstractUnitEntityView.OverrideRotatablePart)
		{
			abstractUnitEntityView.transform.rotation = Quaternion.Euler(0f, unit.Movable.PreviousOrientation, 0f);
		}
		else
		{
			abstractUnitEntityView.OverrideRotatablePart.transform.rotation = Quaternion.Euler(0f, unit.Movable.PreviousOrientation, 0f);
		}
		if ((bool)abstractUnitEntityView.OverrideRotatablePart && !s_IsStartRotate)
		{
			if (unit.View.gameObject != null && unit.Blueprint.VisualSettings?.TurettRotateStart != null)
			{
				SoundEventsManager.PostEvent(unit.Blueprint.VisualSettings.TurettRotateStart, unit.View.gameObject);
				s_IsStartRotate = true;
			}
			else
			{
				PFLog.TechArt.Warning("unit.View.gameObject or unit.Blueprint.VisualSettings.TurettRotateStart is null");
			}
		}
		if ((bool)abstractUnitEntityView.OverrideRotatablePart && Mathf.Approximately(abstractUnitEntityView.OverrideRotatablePart.transform.rotation.eulerAngles.y, unit.DesiredOrientation))
		{
			if (unit.View.gameObject != null && unit.Blueprint.VisualSettings?.TurettRotateStop != null)
			{
				SoundEventsManager.PostEvent(unit.Blueprint.VisualSettings.TurettRotateStop, unit.View.gameObject);
				s_IsStartRotate = false;
			}
			else
			{
				PFLog.TechArt.Warning("unit.View.gameObject or unit.Blueprint.VisualSettings.TurettRotateStop is null");
			}
		}
	}
}
