using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Mechanics.Actions;

[TypeId("330ce332a2f8456690072cf514b8529c")]
public class ContextActionJumpToTarget : ContextActionMove
{
	[SerializeField]
	private ContextValue m_Cells;

	[SerializeField]
	private bool m_EndInTargetPoint;

	[SerializeField]
	private bool m_FromPoint;

	[SerializeField]
	private bool CanJumpInPlace;

	[SerializeField]
	[FormerlySerializedAs("Spell")]
	private BlueprintAbilityReference m_Spell;

	public BlueprintAbility Spell => m_Spell?.Get();

	public override string GetCaption()
	{
		return $"Jump direct to {m_TargetPoint}";
	}

	protected override void RunAction()
	{
		GridNodeBase startPoint = base.Caster.Position.GetNearestNodeXZ();
		GridNodeBase endPoint = GetEndNode(m_TargetPoint.GetValue(), base.Caster, base.Caster.Position);
		EventBus.RaiseEvent(delegate(IUnitGetAbilityJump h)
		{
			h.HandleUnitResultJump(startPoint.CellDistanceTo(endPoint), endPoint?.Vector3Position() ?? m_TargetPoint.GetValue(), base.Caster, base.Caster, useAttack: false);
		});
		EventBus.RaiseEvent(delegate(IUnitJumpHandler h)
		{
			h.HandleUnitJump(startPoint.CellDistanceTo(endPoint), startPoint?.Vector3Position() ?? m_TargetPoint.GetValue(), endPoint?.Vector3Position() ?? m_TargetPoint.GetValue(), base.Caster, base.Context.SourceAbilityBlueprint);
		});
		if (Spell != null)
		{
			MechanicEntity caster = base.Caster;
			if (caster == null)
			{
				Element.LogError(this, "Caster is missing");
				return;
			}
			PartUnitCommands commandsOptional = caster.GetCommandsOptional();
			UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(CreateAbility(m_Spell, base.Context.AsAbilityContext), base.Context.ClickedTarget)
			{
				FreeAction = true
			};
			commandsOptional?.AddToQueue(cmdParams);
		}
	}

	private AbilityData CreateAbility(BlueprintAbilityReference ability, AbilityExecutionContext context)
	{
		return new AbilityData(ability, context.Caster)
		{
			OverrideWeapon = context.Ability.Weapon
		};
	}

	private GridNodeBase GetEndNode(Vector3 targetPosition, MechanicEntity caster, Vector3 casterPosition)
	{
		GridNodeBase nearestNodeXZ = casterPosition.GetNearestNodeXZ();
		GridNodeBase nearestNodeXZ2 = targetPosition.GetNearestNodeXZ();
		int num = m_Cells.Calculate(base.Context);
		GridNodeBase gridNodeBase = (m_FromPoint ? (targetPosition + (casterPosition - targetPosition).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ() : (casterPosition + (targetPosition - casterPosition).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ());
		if (m_EndInTargetPoint && nearestNodeXZ.CellDistanceTo(nearestNodeXZ2) < num)
		{
			gridNodeBase = nearestNodeXZ2;
		}
		if (!caster.CanStandHere(gridNodeBase) || (!CanJumpInPlace && gridNodeBase == nearestNodeXZ))
		{
			List<GridNodeBase> list = new List<GridNodeBase>();
			foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(gridNodeBase, gridNodeBase.GetFirstUnit()?.SizeRect ?? SizePathfindingHelper.GetRectForSize(Size.Medium), Math.Max(caster.SizeRect.Height, caster.SizeRect.Width)))
			{
				if (!caster.CanStandHere(item) || (!CanJumpInPlace && gridNodeBase == nearestNodeXZ))
				{
					continue;
				}
				foreach (GridNodeBase node in GridAreaHelper.GetNodes(item, caster.SizeRect))
				{
					if (node.CellDistanceTo(gridNodeBase) <= 1)
					{
						list.Add(item);
						break;
					}
				}
			}
			GridNodeBase gridNodeBase2 = null;
			int num2 = 1000;
			foreach (GridNodeBase item2 in list)
			{
				if (nearestNodeXZ.CellDistanceTo(item2) <= num2)
				{
					num2 = nearestNodeXZ.CellDistanceTo(item2);
					gridNodeBase2 = item2;
				}
			}
			gridNodeBase = gridNodeBase2;
		}
		return gridNodeBase;
	}

	public override bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		return GetEndNode(target.HasEntity ? target.Entity.Position : target.Point, caster, casterPosition) != null;
	}
}
