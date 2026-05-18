using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitMoveContinuously : UnitCommand<UnitMoveContinuouslyParams>
{
	public override bool IsMoveUnit => true;

	public override bool DontWaitForHands => true;

	public UnitMoveContinuously([NotNull] UnitMoveContinuouslyParams @params)
		: base(@params)
	{
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		if (base.Executor?.View != null)
		{
			base.Executor.View.MovementAgent.SetUpDirectionalMovementParams(base.Params.Direction, base.Params.Multiplier);
		}
	}

	protected override void OnTick()
	{
		UnitMovementAgent unitMovementAgent = base.Executor?.MovementAgent;
		if (!unitMovementAgent)
		{
			OnEnded();
			return;
		}
		if (base.Target != null && (base.Executor.Position - base.Target.Point).sqrMagnitude < 0.01f)
		{
			OnEnded();
			return;
		}
		unitMovementAgent.SetUpDirectionalMovementParams(base.Params.Direction, base.Params.Multiplier);
		UpdateMovementType(base.Params.Multiplier);
		if (!unitMovementAgent.IsReallyMoving && !unitMovementAgent.IsTraverseInProgress)
		{
			OnEnded();
		}
	}

	private void UpdateMovementType(float multiplier)
	{
		base.Params.MovementType = DirectionalMovementStrategy.GetMovementType(multiplier);
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}
}
