using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("a5d7b924bf7743f483c173754bdc072a")]
public class ContextActionRemoveGroupCooldown : ContextAction
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup.Get();

	public override string GetCaption()
	{
		return "Reset cooldown for " + m_AbilityGroup.Get().name + " ability group";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			Element.LogError(this, "Target is missing");
		}
		else
		{
			entity.GetAbilityCooldownsOptional()?.RemoveGroupCooldown(AbilityGroup);
		}
	}
}
