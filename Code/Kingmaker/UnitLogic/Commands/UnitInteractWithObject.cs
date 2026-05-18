using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Covers;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitInteractWithObject : UnitCommand<UnitInteractWithObjectParams>
{
	private AbstractInteractionPart m_Interaction;

	[JsonProperty]
	public Vector3? OverrideTarget { get; set; }

	public AbstractInteractionPart Interaction => m_Interaction ?? (m_Interaction = base.Params.Interaction);

	private MapObjectEntity TargetObject => Interaction.Owner;

	public override bool ShouldBeInterrupted => !Interaction.CanInteract();

	public override bool IsMoveUnit => false;

	public override bool IsUnitEnoughClose
	{
		get
		{
			if (!Interaction.IsEnoughCloseForInteraction(base.Executor))
			{
				return false;
			}
			if (base.NeedLoS)
			{
				return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(base.Executor, base.ApproachPoint, default(IntRect)) != LosCalculations.CoverType.LosBlocker;
			}
			return true;
		}
	}

	public UnitInteractWithObject([NotNull] UnitInteractWithObjectParams @params)
		: base(@params)
	{
	}

	public static void ApproachAndInteract(BaseUnitEntity unit, AbstractInteractionPart interaction, IInteractionVariantActor variantActor = null)
	{
		if (unit != null && interaction.HasEnoughActionPoints(unit))
		{
			if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				UnitCommandsRunner.TryApproachAndInteract(unit, interaction, variantActor);
			}
			else if (interaction.IsEnoughCloseForInteraction(unit))
			{
				unit.Commands.AddToQueueFirst(new UnitInteractWithObjectParams(interaction, variantActor)
				{
					IsSynchronized = true
				});
			}
		}
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		UpdateTarget();
	}

	protected override Vector3 GetTargetPoint()
	{
		return OverrideTarget ?? TargetObject.View.ViewTransform.position;
	}

	private void UpdateTarget()
	{
		if (!OverrideTarget.HasValue && TargetObject is TrapObjectData { ScriptZone: not null } trapObjectData)
		{
			Vector3 normalized = (trapObjectData.ViewPosition - base.Executor.Position).normalized;
			Vector3 point = base.Executor.Position + normalized * (base.Executor.Corpulence + 0.25f);
			if (trapObjectData.ScriptZone.ContainsPosition(point))
			{
				OverrideTarget = base.Executor.Position + normalized * 0.01f;
			}
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Interaction.PlayStartSound(base.Executor);
		if (base.Executor.IsInCombat)
		{
			base.Executor.CombatState.SpendActionPoints(Interaction.ActionPointsCost);
			EventBus.RaiseEvent(delegate(IUnitActionPointsHandler h)
			{
				h.HandleActionPointsSpent(base.Executor);
			});
		}
	}

	protected override void TriggerAnimation()
	{
		base.TriggerAnimation();
		if (Interaction.UseAnimationState != 0)
		{
			StartAnimation(UnitAnimationType.Interact, delegate(UnitAnimationActionHandle h)
			{
				h.InteractionType = Interaction.UseAnimationState;
			});
		}
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		if (Interaction?.InteractionStopSound != null && Interaction.InteractionStopSound != "")
		{
			SoundEventsManager.PostEvent(Interaction.InteractionStopSound, Interaction.View?.gameObject);
		}
	}

	protected override ResultType OnAction()
	{
		if (Interaction.NotInCombat && base.Executor.IsInCombat)
		{
			return ResultType.Fail;
		}
		if (!Interaction.CanInteract())
		{
			return ResultType.Fail;
		}
		AbstractInteractionPart interaction = Interaction;
		InteractionVariativePart variative = interaction as InteractionVariativePart;
		if (variative != null)
		{
			variative.SetVisited();
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(variative);
			});
			return ResultType.Success;
		}
		if (UtilityInteracts.VariativeInteractionCount(Interaction.View) > 0 && base.Params.VariantActor == null)
		{
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(Interaction.View.Data);
			});
			return ResultType.Success;
		}
		IInteractionVariantActor interactionVariantActor = base.Params.VariantActor as IInteractionVariantActor;
		if (interactionVariantActor == null && Interaction is IHasInteractionVariantActors { InteractThroughVariants: not false })
		{
			interactionVariantActor = Interaction.View.Data.Parts.GetAll<IInteractionVariantActor>().FirstOrDefault((IInteractionVariantActor x) => x is KeyRestrictionPart && x.CheckRestriction(base.Executor));
		}
		using (ContextData<InteractionVariantData>.Request().Setup(interactionVariantActor))
		{
			Interaction.Interact(base.Executor);
		}
		return ResultType.Success;
	}
}
