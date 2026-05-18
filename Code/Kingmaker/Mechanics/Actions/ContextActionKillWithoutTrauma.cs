using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[TypeId("713a3fe5623741bdbbd8754f752fc9c0")]
public class ContextActionKillWithoutTrauma : ContextAction
{
	public override string GetCaption()
	{
		return "Kill target without trauma.";
	}

	protected override void RunAction()
	{
		PartLifeState partLifeState = base.Target.Entity?.GetLifeStateOptional();
		PartHealth partHealth = base.Target.Entity?.GetHealthOptional();
		if (partLifeState == null || partHealth == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
		}
		else if (base.Context.Caster == null || !base.Context.Caster.IsAttackingGreenNPC(base.Target.Entity))
		{
			EventBus.RaiseEvent(delegate(IUIContextActionKillHandler h)
			{
				h.HandleOnContextActionKill(base.Context.Caster, base.Target.Entity, base.Context.Blueprint as BlueprintMechanicEntityFact, ContextData<SavingThrowData>.Current?.Rule);
			});
			partLifeState.MarkedForDeath = true;
		}
	}
}
