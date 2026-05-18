using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbilityModifier))]
[TypeId("52d40f7b48b06b1479f4c67a02a617cb")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Ability,
	ContextField.Pattern
})]
[ContextRoleForField("Actions", ContextField.CurrentEntity, "halo iteration entity")]
[ContextRoleForField("Actions", ContextField.Target, "halo iteration entity (same as CurrentEntity)", Note = "for the originally clicked target use ContextMainTarget")]
public class AbilityHaloEffect : BlueprintComponent
{
	public ActionList Actions;

	public int HaloSize;

	public void Apply(AbilityExecutionContext context)
	{
		context.ConfigureHaloPattern(HaloSize);
		HashSet<MechanicEntity> hashSet = new HashSet<MechanicEntity>();
		foreach (MechanicEntity targetableEntity in Game.Instance.EntityPools.TargetableEntities)
		{
			if (context.Ability.IsValidTargetForAttack(targetableEntity) && targetableEntity != context.Caster && AoEPatternHelper.WouldTargetEntityPattern(targetableEntity, context.Pattern, context.MaybeCaster.Position, 1000f, out var _) && hashSet.Add(targetableEntity) && Actions.HasActions)
			{
				using (EvalContext.PushContext(context, targetableEntity))
				{
					Actions.Run();
				}
			}
		}
	}
}
