using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Cohesion;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("1db3c9678380c734f82317b907ff7534")]
public class TutorialTriggerHologramInEnemyCohesion : TutorialTrigger, IHologramPositionChangedHandler, ISubscriber, IHologramClearHandler
{
	private GridNodeBase _lastCheckedNode;

	private bool m_IsTriggered;

	public void HandleHologramPositionChanged()
	{
		if (m_IsTriggered)
		{
			return;
		}
		Vector3? realHologramPosition = UnitPredictionManager.RealHologramPosition;
		if (!realHologramPosition.HasValue)
		{
			return;
		}
		GridNodeBase hologramNode = realHologramPosition.Value.GetNearestNodeXZ();
		if (hologramNode == null || hologramNode == _lastCheckedNode)
		{
			return;
		}
		BaseUnitEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit as BaseUnitEntity;
		if (currentUnit == null || !currentUnit.IsPlayerFaction)
		{
			return;
		}
		BaseUnitEntity cohesionOwner = TutorialEnemyCohesionQuery.FindEnemyOwner((PartCohesion c) => c.PatternNodes?.Contains(hologramNode) ?? false);
		if (cohesionOwner != null)
		{
			_lastCheckedNode = hologramNode;
			TryToTrigger(null, delegate(TutorialContext ctx)
			{
				ctx.SourceUnit = currentUnit;
				ctx.TargetUnit = cohesionOwner;
			});
			m_IsTriggered = true;
		}
	}

	public void HandleHologramClear()
	{
		_lastCheckedNode = null;
	}

	protected override void OnDeactivate()
	{
		_lastCheckedNode = null;
	}
}
