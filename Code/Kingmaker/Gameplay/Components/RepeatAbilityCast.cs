using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
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
			object obj;
			if (command.Target?.Entity == null)
			{
				obj = null;
			}
			else
			{
				IDisposable disposable = EvalContext.Current.PushTarget(command.Target.Entity);
				obj = disposable;
			}
			using (obj)
			{
				OnRepeated.Run();
				return;
			}
		}
		if (command is UnitUseAbility { ExecutionProcess: { } executionProcess, Params: { } @params })
		{
			TargetWrapper clickedTarget = executionProcess.Context.ClickedTarget;
			if (AbilityRestriction.IsPassed(base.Context, base.Owner, clickedTarget, null, @params.Ability))
			{
				UnitUseAbilityParams unitUseAbilityParams = @params.Copy();
				unitUseAbilityParams.IgnoreCooldown = true;
				unitUseAbilityParams.FreeAction = true;
				componentData.RepeatCommandHandle = base.Owner.Commands.AddToQueueFirst(unitUseAbilityParams);
			}
		}
	}
}
