using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitMoveTo : AbstractUnitCommand<UnitMoveToParams>
{
	[JsonProperty]
	public bool PreventInterrupt { get; set; }

	public new Vector3 Target => base.Target?.Point ?? Vector3.zero;

	public override bool IsMoveUnit => true;

	public override bool AwaitMovementFinish => true;

	public override bool IsInterruptible => !(base.Executor.MaybeMovementAgent?.IsTraverseInProgress ?? false);

	public override bool DontWaitForHands => true;

	public float? Orientation => base.Params.Orientation;

	public float ApproachRadiusForAgentASP => base.Params.ApproachRadiusForAgentASP;

	public float MaxApproachForAgentASP => base.Params.MaxApproachForAgentASP;

	public bool Roaming => base.Params.Roaming;

	public UnitMoveTo([NotNull] UnitMoveToParams @params)
		: base(@params)
	{
	}

	public override void TurnToTarget()
	{
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (!base.Executor.CanMove)
		{
			PFLog.Default.Error("UnitMoveTo failed to start. Executor cannot move.");
			ForceFinish(ResultType.Fail);
			return;
		}
		base.Executor.MoveTo(base.Params.ForcedPath, base.Params.Target.Point, ApproachRadiusForAgentASP);
		using (ProfileScope.New("HandleUnitCommandDidAct"))
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Executor, (Action<IUnitCommandActHandler>)delegate(IUnitCommandActHandler h)
			{
				h.HandleUnitCommandDidAct(this);
			}, isCheckRuntime: true);
		}
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (!base.Executor.CanMove)
		{
			ForceFinish(ResultType.Fail);
		}
		else if (!base.Executor.IsReallyMoving)
		{
			ForceFinish(((base.Executor.Position - Target).To2D().magnitude > MaxApproachForAgentASP) ? ResultType.Fail : ResultType.Success);
		}
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		base.Executor.StopMoving();
		if (Orientation.HasValue)
		{
			base.Executor.DesiredOrientation = Orientation.Value;
		}
		if (!TurnController.IsInTurnBasedCombat())
		{
			AbstractUnitEntity executor = base.Executor;
			if (executor != null && executor.IsPlayerFaction && executor.IsInCombat)
			{
				base.Executor.HoldState = true;
			}
		}
	}
}
