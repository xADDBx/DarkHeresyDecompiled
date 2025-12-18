using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.View.Scene.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

[KnowledgeDatabaseID("8c00905928b4480793858c10e26e6a3d")]
public class BaseCoverEntityView : AbstractDestructibleEntityView
{
	protected override bool HasHighlight => true;

	private bool DebugHighlightCovers => Game.Instance.Controllers.InteractionHighlightController?.DebugHighlightCovers ?? false;

	protected override bool GlobalHighlighting
	{
		get
		{
			if (!base.GlobalHighlighting)
			{
				return DebugHighlightCovers;
			}
			return true;
		}
	}

	public override bool CanBeAttackedDirectly => true;

	protected override bool CheckHighlightConditions()
	{
		if (!base.CheckHighlightConditions() || !TurnController.IsInTurnBasedCombat())
		{
			return DebugHighlightCovers;
		}
		return true;
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawIcon(base.transform.position, "Shield_Gizmo");
	}
}
