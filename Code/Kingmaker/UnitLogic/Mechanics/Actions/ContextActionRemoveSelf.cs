using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("1e0ebe55f7204066b7cdb0eb124b863a")]
public class ContextActionRemoveSelf : ContextAction
{
	public override string GetCaption()
	{
		return "Remove self";
	}

	protected override void RunAction()
	{
		if (SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.Fact is Buff buff && buff.Blueprint == base.Owner)
		{
			buff.Remove();
			return;
		}
		AreaEffectEntity current = SimpleContextData<AreaEffectEntity, MechanicsContext.Scope.AreaEffect>.Current;
		if (current != null && current.Blueprint == base.Owner)
		{
			current.ForceEnd();
			return;
		}
		Element.LogError(this, "RemoveSelf can only apply to buffs or area effects! Context.AssociatedBlueprint = {0}", base.Context?.Blueprint);
	}
}
