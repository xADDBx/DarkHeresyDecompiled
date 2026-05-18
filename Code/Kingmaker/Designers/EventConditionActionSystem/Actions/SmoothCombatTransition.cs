using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Smooth Combat Transition")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("b0907160d5b277e4d87d4bd0e7d6370b")]
public class SmoothCombatTransition : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public float Duration = 0.3f;

	public override string GetCaption()
	{
		return $"Smooth Combat Transition ({Duration}s)";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity abstractUnitEntity = Unit?.GetValue();
		if (abstractUnitEntity == null)
		{
			PFLog.Default.Error("[SmoothCombatTransition] Unit is null!");
			return;
		}
		if (!(abstractUnitEntity is UnitEntity unitEntity))
		{
			PFLog.Default.Error("[SmoothCombatTransition] Unit is not UnitEntity!");
			return;
		}
		Vector3 sizePositionOffset = SizePathfindingHelper.GetSizePositionOffset(unitEntity, inBattle: true);
		Vector3 vector = abstractUnitEntity.View?.transform.position ?? abstractUnitEntity.Position;
		Vector3 vector2 = vector - sizePositionOffset;
		PFLog.Default.Log("[SmoothCombatTransition] Unit=" + abstractUnitEntity.View?.name);
		PFLog.Default.Log($"[SmoothCombatTransition] CurrentViewPos={vector}, CombatOffset={sizePositionOffset}");
		PFLog.Default.Log($"[SmoothCombatTransition] OldMechanicsPos={abstractUnitEntity.Position}, NewMechanicsPos={vector2}");
		abstractUnitEntity.Position = vector2;
		PartPreventSnapToGrid orCreate = unitEntity.GetOrCreate<PartPreventSnapToGrid>();
		orCreate.ShouldPreventSnapToGrid = false;
		orCreate.UseCombatOffset = true;
		if (abstractUnitEntity.View != null)
		{
			abstractUnitEntity.View.InterpolationHelper?.ForceUpdatePosition(vector, abstractUnitEntity.Orientation);
			abstractUnitEntity.Movable.ForceHasMotion = true;
		}
		PFLog.Default.Log($"[SmoothCombatTransition] Done. View should stay at {vector}");
	}
}
