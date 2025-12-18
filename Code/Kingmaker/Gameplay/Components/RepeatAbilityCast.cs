using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b9f3feec1f3f40dda4d0896c39b61fc0")]
public sealed class RepeatAbilityCast : UnitFactComponentDelegate, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandEndHandler, EntitySubscriber>
{
	private sealed class ComponentData : IEntityFactComponentTransientData
	{
		[CanBeNull]
		public UnitCommandHandle RepeatCommandHandle;
	}

	public RestrictionCalculator AbilityRestriction = new RestrictionCalculator();

	public ActionList OnRepeated = new ActionList();

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.RepeatCommandHandle?.Cmd == command)
		{
			componentData.RepeatCommandHandle = null;
			using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.SetIfNotNull(command.Target?.Entity))
			{
				OnRepeated.Run();
				return;
			}
		}
		if (command is UnitUseAbility { ExecutionProcess: { } executionProcess, Params: { } @params })
		{
			AbilityExecutionContext context = executionProcess.Context;
			TargetWrapper clickedTarget = executionProcess.Context.ClickedTarget;
			if (AbilityRestriction.IsPassed(context, base.Owner, clickedTarget))
			{
				UnitUseAbilityParams unitUseAbilityParams = @params.Copy();
				unitUseAbilityParams.IgnoreCooldown = true;
				unitUseAbilityParams.FreeAction = true;
				componentData.RepeatCommandHandle = base.Owner.Commands.AddToQueueFirst(unitUseAbilityParams);
			}
		}
	}
}
