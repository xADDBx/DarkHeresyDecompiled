using JetBrains.Annotations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitDirectInteract : UnitCommand<UnitDirectInteractParams>
{
	public AbstractInteractionPart Interaction => base.Params.Interaction;

	public override bool IsMoveUnit => false;

	public override bool IsUnitEnoughClose => true;

	public UnitDirectInteract([NotNull] UnitDirectInteractParams @params)
		: base(@params)
	{
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (base.Executor.IsInCombat)
		{
			base.Executor.CombatState.SpendActionPoints(Interaction.ActionPointsCost);
			EventBus.RaiseEvent(delegate(IUnitActionPointsHandler h)
			{
				h.HandleActionPointsSpent(base.Executor);
			});
		}
	}

	protected override ResultType OnAction()
	{
		AbstractInteractionPart interaction = Interaction;
		if (interaction == null || !interaction.CanInteract())
		{
			return ResultType.Fail;
		}
		interaction.Interact(base.Executor);
		return ResultType.Success;
	}

	public static UnitDirectInteractParams CreateCommandParams(AbstractInteractionPart interaction)
	{
		return new UnitDirectInteractParams(interaction)
		{
			IsSynchronized = true,
			NeedLoS = false
		};
	}
}
